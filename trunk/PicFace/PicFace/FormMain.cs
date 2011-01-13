﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PicFace.Picasa;
using System.IO;
using PicFace.Generic;
using PicFace.ExifTool;

namespace PicFace
{
   /// <summary>
   /// The main form of the tool
   /// </summary>
   public partial class FormMain : Form
   {
      #region Fields
      /// <summary>
      /// All contacts
      /// </summary>
      private ContactTable _Contacts;
      /// <summary>
      /// Name of the current directory
      /// </summary>
      private string _CurrentDirectory;
      /// <summary>
      /// Picture List
      /// </summary>
      private PictureList _PictureList;
      /// <summary>
      /// The object to visualise a face
      /// </summary>
      private FaceToPictureBox _FaceVisualiser;
      #endregion
      /// <summary>
      /// Constructor
      /// </summary>
      public FormMain()
      {
         InitializeComponent();
      }
      /// <summary>
      /// Form is being loaded
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void FormMain_Load(object sender, EventArgs e)
      {
         // Refresh contacts first
         buttonRefreshContacts_Click(sender, e);
         _FaceVisualiser = new FaceToPictureBox(pictureBoxPreview);

#if DEBUG
         //// ###############
         _CurrentDirectory = @"C:\Users\Christian\Pictures\Tests";
         textBoxDirectory.Text = _CurrentDirectory;

         LoadPictureIndex();
         //// ###############
#endif
      }
      /// <summary>
      /// Refresh the contacts
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void buttonRefreshContacts_Click(object sender, EventArgs e)
      {
         labelContactsFile.Text = ContactReader.PicasaDefaultPersonFile;

         _Contacts = ContactReader.Read(ContactReader.PicasaDefaultPersonFile);
         listBoxContacts.Items.Clear();

         foreach (KeyValuePair<string, Contact> c in _Contacts)
         {
            listBoxContacts.Items.Add(c.Value);
         }
         labelContactNb.Text = listBoxContacts.Items.Count.ToString() + " Contacts";
      }
      /// <summary>
      /// Show info about the contact when selecting a new one
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void listBoxContacts_SelectedIndexChanged(object sender, EventArgs e)
      {
         Contact c = listBoxContacts.SelectedItem as Contact;
         if (c != null)
         {
            labelContactInfo.Text = c.Name;
            if (c.Display.Length > 0)
            {
               labelContactInfo.Text += " (\"" + c.Display + "\")";
            }
            labelContactInfo.Text += " " + c.Modified.ToShortDateString() + " " + c.Modified.ToShortTimeString();
            labelContactInfo.Text += " id:" + c.Id;
         }
         else
         {
            labelContactInfo.Text = "";
         }
      }
      /// <summary>
      /// Select Directory in "My Pictures"
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void selectDirectoryInMyPicturesToolStripMenuItem_Click(object sender, EventArgs e)
      {
         folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyPictures;
         DialogResult res = folderBrowserDialog1.ShowDialog();
         if (res == DialogResult.OK)
         {
            _CurrentDirectory = folderBrowserDialog1.SelectedPath;
            textBoxDirectory.Text = _CurrentDirectory;

            LoadPictureIndex();
         }
      }
      /// <summary>
      /// Select any Directory
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void selectDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
      {
         folderBrowserDialog1.RootFolder = Environment.SpecialFolder.MyComputer;
         DialogResult res = folderBrowserDialog1.ShowDialog();
         if (res == DialogResult.OK)
         {
            _CurrentDirectory = folderBrowserDialog1.SelectedPath;
            textBoxDirectory.Text = _CurrentDirectory;

            LoadPictureIndex();
         }
      }
      /// <summary>
      /// Load the Picture Index
      /// </summary>
      private void LoadPictureIndex()
      {
         _PictureList = new PictureList(_CurrentDirectory, _Contacts);

         listBoxFilesChanged.Items.Clear();
         foreach (KeyValuePair<string, PictureComparer> p in _PictureList.ConsolidatedList)
         {
            listBoxFiles.Items.Add(p.Value);

            if (p.Value.ExifUpdateNeeded)
            {
               listBoxFilesChanged.Items.Add(p.Value);
            }
         }
      }
      /// <summary>
      /// Selected Index changed: load the picture and show who is on it
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void listBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
      {
         PictureComparer p = listBoxFilesChanged.SelectedItem as PictureComparer;
         if (p != null)
         {
            _FaceVisualiser.FileName = Path.Combine(_CurrentDirectory, p.FileName);
            // Add all faces to the found faces list boxes
            listBoxPersonsFound.Items.Clear();
            listBoxPersonsFoundXmp.Items.Clear();

            if (p.PicasaInfo != null)
            {
               foreach (KeyValuePair<string, Face> kp in p.PicasaInfo.Faces)
               {
                  listBoxPersonsFound.Items.Add(kp.Value);
               }
            }
            if (p.ExifInfo != null)
            {
               foreach (KeyValuePair<string, Face> kp in p.ExifInfo.Faces)
               {
                  listBoxPersonsFoundXmp.Items.Add(kp.Value);
               }
            }
         }
      }
      /// <summary>
      /// Index of the list changed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void listBoxPersonsFound_SelectedIndexChanged(object sender, EventArgs e)
      {
         ListBox listbox = sender as ListBox;
         if (listbox != null)
         {
            Face f = listbox.SelectedItem as Face;
            if (f != null)
            {
               _FaceVisualiser.DrawFace(f);
            }
         }
      }
      /// <summary>
      /// Mouse moves over picture
      /// </summary>
      /// <param name="sender">The picture box the mouse is moving on</param>
      /// <param name="e">Event args</param>
      private void pictureBoxPreview_MouseMove(object sender, MouseEventArgs e)
      {

      }
   }
}