using Accounting.DTO;
using Accounting.IRepositories.Koperasi;
using Accounting.Repositories.SQLLite.AccountngRepository;
using Accounting.Services.JurnalServices;
using ControlzEx.Standard;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using static ClosedXML.Excel.XLPredefinedFormat;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Accounting.Domain.Entities
{
    // belummemakai parent child model
    // tiap variant Satu baris
    internal class Product
    {
        public int Jenis { set; get; }

        public int Id { set; get; }
        public string Code { set; get; }
        public string Name { set; get; }
        public decimal PurchasePrice{ set; get; }
        public decimal CurrentSellingPrice { set; get; }
        public int  CurrentStock{ set; get; } 
        public string CreatedBy { set; get; } = string.Empty;
    }
}
