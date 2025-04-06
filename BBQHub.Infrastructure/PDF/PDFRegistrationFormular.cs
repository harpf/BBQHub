using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.PDF;
using QuestPDF.Fluent;

namespace BBQHub.Infrastructure.PDF
{
    public class PDFRegistrationFormular
    {
        public byte[] Generate(TeilnahmePdfData model)
        {
            var document = new TeilnahmeDocument(model);
            return document.GeneratePdf();
        }
    }
}