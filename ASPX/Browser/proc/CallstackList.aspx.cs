﻿/*
 * Crashweb
 * https://github.com/NtreevSoft/Crashweb
 * 
 * Released under the MIT License.
 * 
 * Copyright (c) 2008 Ntreev Soft co., Ltd.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
 * persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
 * Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */ 

using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Browser.mod;

namespace Browser.proc
{
    public partial class CallstackList : System.Web.UI.Page
    {
        private void WriteCData(System.Xml.XmlTextWriter writer, string tag, string value)
        {
            writer.WriteStartElement(tag);
            writer.WriteCData(value);
            writer.WriteEndElement();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.ContentType = "text/xml";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            int project_uid;
            int pageNo;
            int pageSize;
            bool seperate_version;
            string specific_version;
            int hideResolved = 0;
            int fromDate;
            int toDate;

            if (int.TryParse(Request.QueryString["project"], out project_uid) == false)
                project_uid = 1;

            if (int.TryParse(Request.QueryString["pageNo"], out pageNo) == false)
                pageNo = 1;

            if (int.TryParse(Request.QueryString["pageSize"], out pageSize) == false)
                pageSize = 30;

            if (int.TryParse(Request.QueryString["from"], out fromDate) == false)
                fromDate = 0;

            if (int.TryParse(Request.QueryString["to"], out toDate) == false)
                toDate = 0;

            if (bool.TryParse(Request.QueryString["sv"], out seperate_version) == false)
                seperate_version = false;

            specific_version = Request.QueryString["ver"];
            if (specific_version != null)
            {
                specific_version.Trim();
                if (specific_version.Length <= 0)
                    specific_version = null;
            }

            string temp1 = Request.QueryString["hideResolved"];
            string temp2 = Request.QueryString["hideExamination"];
            Boolean check1 = false;
            Boolean check2 = false;
            if (temp1 != null)
                check1 = Boolean.Parse(temp1);
            if (temp2 != null)
                check2 = Boolean.Parse(temp2);
            if (check1 && check2)
                hideResolved = 3;
            else if (check1)
                hideResolved = 1;
            else if (check2)
                hideResolved = 2;

            using (System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(Response.OutputStream, System.Text.Encoding.UTF8))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Callstack");
                writer.WriteAttributeString("project", project_uid.ToString());
                writer.WriteAttributeString("pageNo", pageNo.ToString());
                writer.WriteAttributeString("pageSize", pageSize.ToString());
                writer.WriteAttributeString("req", specific_version);

                writer.WriteStartElement("Items");

                DB.ForEachCallstackGroup ItemWriter = delegate(int count, int callstack_uid, string funcname, string version, DateTime latest_time, string relative_time, string assigned, int num_comments)
                {
                    writer.WriteStartElement("Item");

                    writer.WriteAttributeString("count", count.ToString());
                    writer.WriteAttributeString("callstack_uid", callstack_uid.ToString());
                    writer.WriteAttributeString("latest_time", latest_time.ToString());
                    writer.WriteAttributeString("relative_time", relative_time);
                    writer.WriteAttributeString("assigned", assigned);
                    writer.WriteAttributeString("num_comments", num_comments.ToString());

                    this.WriteCData(writer, "Funcname", funcname);
                    this.WriteCData(writer, "Version", version);

                    writer.WriteEndElement();
                };

                int totalPageSize = 0;
                DB.LoadCallstackList(project_uid, pageNo, pageSize, fromDate, toDate, seperate_version, specific_version, hideResolved, ItemWriter, out totalPageSize);

                writer.WriteEndElement(); // Items

                writer.WriteStartElement("Outputs");
                this.WriteCData(writer, "TotalPageSize", totalPageSize.ToString());
                writer.WriteEndElement(); // Outputs

                writer.WriteEndElement(); // Callstack
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
        }
    }
}