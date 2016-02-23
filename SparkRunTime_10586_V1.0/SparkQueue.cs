﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SparkRunTime_10586_V1._0
{
    public class SparkQueue
    {
        public delegate void SparkQueueEventHandler(object sender, EventArgs e);
        public event SparkQueueEventHandler DataReadyForPickUp;
        public int QueueCycleMilliSeconds { get; set; }
        public string SubDirectoryPath { get; set; }
        public string DataFileName { get; set; }

        private Queue inboundQueue;
        private Queue outboundQueue;
        private Timer InboundDataTimer;
        private Timer OutboundDataTimer;
        private Object FILELOCK = new Object();
        public StorageFolder folder;
        public StorageFile file;
        public List<string> Errors;
        public List<string> ObjectMessages;


        private SemaphoreSlim _syncLock = new SemaphoreSlim(1);


        public SparkQueue()
        {

            //initializeClass();
            this.Errors = new List<string>();
            this.ObjectMessages = new List<string>();

        }
        public async Task itializeClass()
        {
            QueueCycleMilliSeconds = 500;
            this.folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {

                this.file = folder.CreateFileAsync("SparkQueueDB.txt", CreationCollisionOption.OpenIfExists).AsTask().Result;

            }
            catch (Exception ex)
            {
                this.Errors.Add("Queue Failed To Initialize");
                this.Errors.Add(ex.Message.ToString());

            }

            inboundQueue = new Queue();
            outboundQueue = new Queue();

            //if (Program.strPowerOuttageMissedDownEvent.Length > 1)
            //{
            //    this.Enqueue(Program.strPowerOuttageMissedDownEvent);
            //    Program.strPowerOuttageMissedDownEvent = "";
            //}

            InboundDataTimer = new Timer(new TimerCallback(ProcessInboundEvent), new Object(), 250, 250);
            OutboundDataTimer = new Timer(new TimerCallback(ProcessOutboundEvent), new Object(), 250, 250);

            this.file = null;
            this.folder = null;
        }

        private async void ProcessInboundEvent(object o)
        {

            _syncLock.Wait(30000);

            try
            {
                Debug.WriteLine("Check For Inbound");
                while (inboundQueue.Count > 0)
                {
                    Debug.WriteLine("YES - Inbound Exists");
                    var line = inboundQueue.Peek().ToString();
                    bool success;
                    try
                    {
                        success = await writeDataToFileAsync(line);
                    }
                    catch (Exception exc)
                    {
                        this.Errors.Add(exc.Message.ToString());
                        success = false;
                        throw;
                    }


                    if (success == true)
                    {
                        inboundQueue.Dequeue();
                    }
                }
            }
            catch (Exception ex)
            {
                this.Errors.Add("SparkQueue Exception - L108");
                this.Errors.Add(ex.Message.ToString());
            }
            _syncLock.Release();
        }

        private async void ProcessOutboundEvent(object o)
        {
            _syncLock.Wait(30000);

            try
            {
                Debug.WriteLine("Check For Outbound");
                Debug.WriteLine("Outbound Queue: " + outboundQueue.Count.ToString());

                if (outboundQueue.Count == 0)
                {
                    //Debug.Print("There is nothing in Outbound Queue.  Check if there is anything on the SD Card");

                    await readDataFromFile();
                }
                else
                {
                    if (DataReadyForPickUp != null)
                    {
                        Debug.WriteLine("Firing DataReadyForPickUp");
                        DataReadyForPickUp(this, new EventArgs());
                    }
                }
            }
            catch (Exception)
            {


            }
            _syncLock.Release();
        }

        private async Task<bool> writeDataToFileAsync(string line)
        {
            bool result = false;
            // var result = new TaskCompletionSource<bool>();
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile dbFile = await folder.CreateFileAsync("SparkQueueDB.txt", CreationCollisionOption.OpenIfExists);
                //Stream taskGetStreamWriter;
                try
                {

                    await FileIO.AppendTextAsync(dbFile, line + Environment.NewLine);
                    Debug.WriteLine("Written to data file: " + line);
                    //    taskGetStreamWriter = dbFile.OpenStreamForWriteAsync().Result;
                }
                catch (Exception _exc1)
                {
                    Debug.WriteLine("ERROR SQ - Line 166: " + _exc1.Message);
                    result = false;
                }


                result = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR SQ - Line 175: " + ex.Message);
                Utilities.SparkEmail.Send("ERROR SQ Line 175 " + Configuration.strAssetNumber + " " + ex.Message);
                result = false;
            }

            return result;
        }
        private async Task<bool> readDataFromFile()
        {
            bool blnSuccessfullyCheckedFoData = false;

            var line = "";
            try
            {
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("SparkQueueDB.txt");

                IList<string> lines = await FileIO.ReadLinesAsync(file);

                if (lines != null && lines.Count > 0)
                {
                    line = lines[0];
                }
                //line = await FileIO.ReadLinesAsync(file);

                blnSuccessfullyCheckedFoData = true;
            }
            catch (Exception ex)
            {
                //this.Errors.Add("ReadDataFromFile Error");
                //this.Errors.Add(ex.Message.ToString());
                Debug.WriteLine("ERROR SQ - Line 205: " + ex.Message);
                blnSuccessfullyCheckedFoData = false;
                Utilities.SparkEmail.Send("ERROR SQ Line 205 " + Configuration.strAssetNumber + " " + ex.Message);

            }
            if (line != null && line.Trim().Length > 10)
            {
                Debug.WriteLine("There is something on the SD Card.  Add it to the outbound queue.");
                outboundQueue.Enqueue(line);

            }

            return blnSuccessfullyCheckedFoData;
        }

        private System.Threading.SemaphoreSlim _removeDataLock = new SemaphoreSlim(1);
        private async Task<bool> removeDataFromFileAsync(string lineToRemove)
        {
            _removeDataLock.Wait(30000);
            bool result = false;

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync("SparkQueueDB.txt");
            var lines = new ArrayList();
            using (var reader = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            try
            {
                file = await folder.CreateFileAsync("SparkQueueDB.txt", CreationCollisionOption.ReplaceExisting);
                using (StreamWriter writer = new StreamWriter(await file.OpenStreamForWriteAsync()))
                {
                    foreach (var l in lines)
                    {
                        if (l.ToString() != lineToRemove)
                        {
                            writer.WriteLine(l);
                        }
                        else
                        {
                            Debug.WriteLine("LINE BEING REMOVED");
                        }
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: SQ Line 256 - " + ex.Message);
                Utilities.SparkEmail.Send("ERROR SQ Line 256 " + Configuration.strAssetNumber + " " + ex.Message);
                throw;
            }
            _removeDataLock.Release();
            return result;
        }
        public void Enqueue(string textToAdd)
        {
            if (inboundQueue != null)
            {
                if (!inboundQueue.Contains(textToAdd) && textToAdd != null)
                {
                    inboundQueue.Enqueue(textToAdd);
                }
            }
            else
            {
                Debug.WriteLine("***********************************************");
                Debug.WriteLine("***********************************************");
                Debug.WriteLine("***********************************************");

            }
        }
        public async Task<bool> Dequeue()
        {
            bool blnSuccess = false;
            var line = "";
            if (outboundQueue.Count > 0)
            {
                line = outboundQueue.Peek().ToString();
                bool blnRecordRemovedFromFile = await removeDataFromFileAsync(line);
                if (blnRecordRemovedFromFile)
                {
                    outboundQueue.Dequeue();
                    blnSuccess = true;
                }
            }
            return blnSuccess;
        }
        public string Peek()
        {
            return ((outboundQueue.Count > 0) ? outboundQueue.Peek().ToString() : "");
        }
        public int Count
        {
            get
            {
                lock (FILELOCK)
                {
                    return GetCountAsync().Result;
                }
            }
        }

        private async Task<int> GetCountAsync()
        {
            await Task.Delay(100);
            int records = 0;

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync("SparkQueueDB.txt");
            StreamReader reader = new StreamReader(await file.OpenStreamForReadAsync());

            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                records++;
            }

            return records;
        }
    }
}

