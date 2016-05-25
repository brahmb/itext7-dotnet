using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using iTextSharp.IO.Font;
using iTextSharp.Kernel.Font;
using iTextSharp.Kernel.Pdf.Canvas;
using iTextSharp.Kernel.Pdf.Layer;
using iTextSharp.Kernel.Utils;
using iTextSharp.Test;

namespace iTextSharp.Kernel.Pdf
{
	public class PdfLayerTest : ExtendedITextTest
	{
		public const String sourceFolder = "../../resources/itextsharp/kernel/pdf/PdfLayerTest/";

		public const String destinationFolder = "test/itextsharp/kernel/pdf/PdfLayerTest/";

		[TestFixtureSetUp]
		public static void BeforeClass()
		{
			CreateDestinationFolder(destinationFolder);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.Exception"/>
		[Test]
		public virtual void TestInStamperMode1()
		{
			PdfDocument pdfDoc = new PdfDocument(new PdfReader(new FileStream(sourceFolder + 
				"input_layered.pdf", FileMode.Open)), new PdfWriter(new FileStream(destinationFolder
				 + "output_copy_layered.pdf", FileMode.Create)));
			pdfDoc.Close();
			NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(destinationFolder
				 + "output_copy_layered.pdf", sourceFolder + "input_layered.pdf", destinationFolder
				, "diff"));
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.Exception"/>
		[Test]
		public virtual void TestInStamperMode2()
		{
			PdfDocument pdfDoc = new PdfDocument(new PdfReader(new FileStream(sourceFolder + 
				"input_layered.pdf", FileMode.Open)), new PdfWriter(new FileStream(destinationFolder
				 + "output_layered.pdf", FileMode.Create)));
			PdfCanvas canvas = new PdfCanvas(pdfDoc, 1);
			PdfLayer newLayer = new PdfLayer("appended", pdfDoc);
			canvas.BeginLayer(newLayer).BeginText().SetFontAndSize(PdfFontFactory.CreateFont(
				FontConstants.HELVETICA), 18).MoveText(200, 600).ShowText("APPENDED CONTENT").EndText
				().EndLayer();
			IList<PdfLayer> allLayers = pdfDoc.GetCatalog().GetOCProperties(true).GetLayers();
			foreach (PdfLayer layer in allLayers)
			{
				if (layer.IsLocked())
				{
					layer.SetLocked(false);
				}
				if ("Grouped layers".Equals(layer.GetTitle()))
				{
					layer.AddChild(newLayer);
				}
			}
			pdfDoc.Close();
			NUnit.Framework.Assert.IsNull(new CompareTool().CompareByContent(destinationFolder
				 + "output_layered.pdf", sourceFolder + "cmp_output_layered.pdf", destinationFolder
				, "diff"));
		}
	}
}