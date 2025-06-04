using ePubEditor.Core.Models;
using ePubEditor.Core.Services;
using FuzzySharp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core
{
    internal class GoogleMetadataFetcher
    {
        private readonly IGoogleBook _googleBook;

        public GoogleMetadataFetcher(IGoogleBook googleBook)
        {
            _googleBook = googleBook;
        }
        public async Task GoogleSearch()
        {
            // Get all epub in the current directory
            List<CalibreMetadata> initialMetadata = Helper.LoadObjectsFromCSV<CalibreMetadata>("inputs");

            string outputPath = "C:\\Users\\smoreau\\Downloads\\Output\\output.csv";
            using (StreamWriter writer = new StreamWriter(outputPath, append: true))
            {
                foreach (CalibreMetadata initialLine in initialMetadata)
                {
                    Debug.WriteLine(initialLine.Uuid);

                    try
                    {
                        BookMetadata? metadata = null;
                        if (!string.IsNullOrWhiteSpace(initialLine.Isbn))
                        {
                            Models.GoogleBook.Result result = await _googleBook.GetBookInfoAsync(initialLine.Isbn);
                            if (result?.Items != null && result.Items.Count > 0)
                            {
                                metadata = BookMetadata.FromGoogleResult(result.Items[0], initialLine.Isbn);
                            }
                        }

                        // We search by tiltle and author
                        if (metadata == null)
                        {
                            Models.GoogleBook.Result result = await _googleBook.GetBookInfoAsync(initialLine.Title, initialLine.Authors);
                            if (result?.Items != null && result.Items.Count > 0)
                            {
                                foreach (Models.GoogleBook.Item item in result?.Items)
                                {
                                    BookMetadata tempmetadata = BookMetadata.FromGoogleResult(item, initialLine.Isbn);
                                    if (tempmetadata.Title == null) continue; // Skip if title is null
                                    int titleScore = Fuzz.Ratio(tempmetadata.Title, initialLine.Title);
                                    if (titleScore < 80) continue;
                                    if (tempmetadata.Authors == null) continue; // Skip if authors are null
                                    int authorScore = Fuzz.Ratio(string.Join(", ", tempmetadata.Authors), initialLine.Authors);
                                    if (authorScore < 80) continue;
                                    metadata = tempmetadata;
                                    break;
                                }
                            }
                        }

                        // We search only by title
                        if (metadata == null)
                        {
                            Models.GoogleBook.Result result = await _googleBook.GetBookInfoFromTitleAsync(initialLine.Title);
                            if (result?.Items != null && result.Items.Count > 0)
                            {
                                foreach (Models.GoogleBook.Item item in result?.Items)
                                {
                                    BookMetadata tempmetadata = BookMetadata.FromGoogleResult(item, initialLine.Isbn);
                                    if (tempmetadata.Title == null) continue; // Skip if title is null
                                    int titleScore = Fuzz.Ratio(tempmetadata.Title, initialLine.Title);
                                    if (titleScore < 80) continue;
                                    metadata = tempmetadata;
                                    break;
                                }
                            }
                        }

                        // We swap tiltle and author
                        if (metadata == null)
                        {
                            Models.GoogleBook.Result result = await _googleBook.GetBookInfoAsync(initialLine.Authors, initialLine.Title);
                            if (result?.Items != null && result.Items.Count > 0)
                            {
                                foreach (Models.GoogleBook.Item item in result?.Items)
                                {
                                    BookMetadata tempmetadata = BookMetadata.FromGoogleResult(item, initialLine.Isbn);
                                    if (tempmetadata.Title == null) continue; // Skip if title is null
                                    int titleScore = Fuzz.Ratio(tempmetadata.Title, initialLine.Authors);
                                    if (titleScore < 80) continue;
                                    if (tempmetadata.Authors == null) continue; // Skip if authors are null
                                    int authorScore = Fuzz.Ratio(string.Join(", ", tempmetadata.Authors), initialLine.Title);
                                    if (authorScore < 80) continue;
                                    metadata = tempmetadata;
                                    break;
                                }
                            }
                        }

                        if (metadata == null)
                        {
                            await writer.WriteLineAsync($"{initialLine.Uuid};;");
                        }
                        else
                        {
                            await writer.WriteLineAsync($"{initialLine.Uuid};{metadata.WriteMetadata()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await writer.WriteLineAsync($"{initialLine.Uuid};{ex.Message};");
                    }

                    await writer.FlushAsync();
                    Debug.WriteLine("Done");


                }
            }
        }
    }
}
