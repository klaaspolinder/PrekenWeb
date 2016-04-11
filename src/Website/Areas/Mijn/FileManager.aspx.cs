﻿using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using IZ.WebFileManager;
using PrekenWeb.Data;

namespace Prekenweb.Website.Areas.Mijn
{
    public partial class FileManagerWebForm : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated || !User.IsInRole("Bestandsbeheer")) throw new Exception("Not authorized");
            if (!IsPostBack)
            {
                using (PrekenwebContext context = new PrekenwebContext())
                {
                    var gebruiker = context.Users.Single(x => x.Email == User.Identity.Name);
                    var gebruikersMap = Server.MapPath(string.Format("~/Content/Preken/Gebruikers/{0}/", gebruiker.Naam));
                    if (!Directory.Exists(gebruikersMap))
                    {
                        Directory.CreateDirectory(gebruikersMap);
                    }

                    if (User.IsInRole("BestandsbeheerPrekenWeb"))
                    {
                        FileManager.RootDirectories.Add(new RootDirectory() { DirectoryPath = "~/Content/Images/Uploads/", Text = Resources.Resources.Afbeeldingen, });
                        FileManager.RootDirectories.Add(new RootDirectory() { DirectoryPath = "~/Content/Preken/", Text = Resources.Resources.PrekenWeb });
                    }
                    else
                    {
                        FileManager.RootDirectories.Add(new RootDirectory() { DirectoryPath = "~/Content/Preken/Algemeen/", Text = Resources.Resources.Algemeen, });
                        FileManager.RootDirectories.Add(new RootDirectory() { DirectoryPath = string.Format("~/Content/Preken/Gebruikers/{0}/", gebruiker.Naam), Text = gebruiker.Naam });
                    }
                }
                //FileManager.Width = Unit.Percentage(100);
            }
        }

    }

}