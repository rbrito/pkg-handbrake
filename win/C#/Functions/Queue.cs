﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace Handbrake.Functions
{
    public class Queue
    {
        ArrayList queue = new ArrayList();
        ArrayList lastQuery;
        int id = 0; // Unique identifer number for each job

        public ArrayList getQueue()
        {
             return queue;
        }

        /// <summary>
        /// Get's the next CLI query for encoding
        /// </summary>
        /// <returns>String</returns>
        public String getNextItemForEncoding()
        {
            Object query = queue[0];
            lastQuery = (ArrayList)query;
            remove(0);    // Remove the item which we are about to pass out.
            return lastQuery[1].ToString();
        }

        /// <summary>
        /// Add's a new item to the queue
        /// </summary>
        /// <param name="query">String</param>
        public void add(string query)
        {
            // Creates a new job with a unique identifer and cli query
            ArrayList newJob = new ArrayList();
            newJob.Add(id);
            newJob.Add(query);
            id++;

            // Adds the job to the queue
            queue.Add(newJob);
        }

        /// <summary>
        /// Removes an item from the queue.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Bolean true if successful</returns>
        public Boolean remove(int index)
        {
            queue.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Returns how many items are in the queue
        /// </summary>
        /// <returns>Int</returns>
        public int count()
        {
            return queue.Count;
        }

        /// <summary>
        /// Get's the last query to be selected for encoding by getNextItemForEncoding()
        /// </summary>
        /// <returns>String</returns>
        public string getLastQuery()
        {
            return lastQuery[1].ToString();
        }

        /// <summary>
        /// Move an item with an index x, up in the queue
        /// </summary>
        /// <param name="index">Int</param>
        public void moveUp(int index)
        {
            if (index != 0)
            {
                ArrayList item = (ArrayList)queue[index];

                queue.Insert((index - 1), item);
                queue.RemoveAt((index + 1));
            }
        }

        /// <summary>
        /// Move an item with an index x, down in the queue
        /// </summary>
        /// <param name="index">Int</param>
        public void moveDown(int index)
        {
            if (index != queue.Count - 1)
            {
                ArrayList item = (ArrayList)queue[index];

                queue.Insert((index + 2), item);
                queue.RemoveAt((index));
            }
        }

        /// <summary>
        /// Writes the current queue to disk. hb_queue_recovery.dat
        /// This function is called after getNextItemForEncoding()
        /// </summary>
        public void write2disk(string file)
        {
            try
            {
                string tempPath = "";
                if (file == "hb_queue_recovery.dat")
                    tempPath = Path.Combine(Path.GetTempPath(), "hb_queue_recovery.dat");
                else
                    tempPath = file;
                using (StreamWriter writer = new StreamWriter(tempPath))
                {
                    foreach (ArrayList item in queue)
                    {
                        writer.WriteLine(item[1].ToString());
                    }
                    writer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                // Any Errors will be out of diskspace/permissions problems. Don't report them as they'll annoy the user.
            }
        }

        /// <summary>
        /// Writes the current queue to disk to the location specified in file
        /// </summary>
        /// <param name="file"></param>
        public void writeBatchScript(string file)
        {
            string queries = "";
            foreach (ArrayList queue_item in queue)
            {
                string q_item = queue_item[1].ToString();
                string fullQuery = '"' + Application.StartupPath.ToString() + "\\HandBrakeCLI.exe" + '"' + q_item;

                if (queries == string.Empty)
                    queries = queries + fullQuery;
                else
                    queries = queries + " && " + fullQuery;
            }
            string strCmdLine = queries;

            if (file != "")
            {
                try
                {
                    // Create a StreamWriter and open the file, Write the batch file query to the file and 
                    // Close the stream
                    StreamWriter line = new StreamWriter(file);
                    line.WriteLine(strCmdLine);
                    line.Close();

                    MessageBox.Show("Your batch script has been sucessfully saved.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to write to the file. Please make sure that the location has the correct permissions for file writing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }

            }
        }

        /// <summary>
        /// Recover the queue from hb_queue_recovery.dat
        /// </summary>
        public void recoverQueue(string file)
        {
            try
            {
                string tempPath = "";
                if (file == "hb_queue_recovery.dat")
                    tempPath = Path.Combine(Path.GetTempPath(), "hb_queue_recovery.dat");
                else
                    tempPath = file;
                using (StreamReader reader = new StreamReader(tempPath))
                {
                    string queue_item = reader.ReadLine();

                    while (queue_item != null)
                    {
                        this.add(queue_item);
                        queue_item = reader.ReadLine();
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("HandBrake was unable to recover the queue. \nError Information:" + exc.ToString(), "Queue Recovery Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
