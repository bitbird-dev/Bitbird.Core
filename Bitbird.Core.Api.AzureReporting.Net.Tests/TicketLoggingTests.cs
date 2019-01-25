using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Bitbird.Core.Api.AzureReporting.Net.Models;
using Bitbird.Core.Api.AzureReporting.Net.Tests.Properties;
using Microsoft.Azure;
using Newtonsoft.Json;

namespace Bitbird.Core.Api.AzureReporting.Net.Tests
{
    [TestClass]
    public class TicketLoggingTests
    {
        private const string ArchiveFileName = "TestArchieve.zip";
        private const string TextFileName = "TestTextFile.txt";
        private const string TextFileContent = "Das ist ein Text.";

        private static IUploadAttachmentModel[] CreateAttachments()
        {
            return new IUploadAttachmentModel[]
            {
                new UploadAttachmentModel
                {
                    Filename = TextFileName,
                    GetStream = () => new MemoryStream(Encoding.UTF8.GetBytes(TextFileContent))
                },
                new UploadAttachmentModel
                {
                    Filename = ArchiveFileName,
                    GetStream = () => new MemoryStream(Resources.TestArchieve)
                }
            };
        }
        private static IEnumerable<ICreateTicketModel> CreateTickets()
        {
            return new ICreateTicketModel[]
            {
                new CreateBugTicketModel
                {
                    Priority = Priority.Unimportant,
                    ReproductionSteps = "Steps to reproduce something..",
                    Severity = Severity.Low,
                    SystemInfo = "Some kind of application",
                    Title = "[UNIT TEST SIDEEFFECT] Some title"
                },
                new CreateFeatureTicketModel
                {
                    Priority = Priority.Unimportant,
                    SystemInfo = "Some kind of application",
                    Title = "[UNIT TEST SIDEEFFECT] Some title",
                    Description = "Some description"
                },
                new CreateUserStoryTicketModel
                {
                    Priority = Priority.Unimportant,
                    SystemInfo = "Some kind of application",
                    Title = "[UNIT TEST SIDEEFFECT] Some title",
                    Description = "Some description",
                    AcceptanceCriteria = "Some acceptance criteria"
                }
            };
        }

        [TestMethod]
        public async Task TestAllMethods()
        {
            if (string.IsNullOrWhiteSpace(CloudConfigurationManager.GetSetting("AzureReporting.AccessToken")))
            {
                Console.WriteLine("TEST NOT EXECUTED. Access token not specified!");
                return;
            }

            var tickets = await TicketLogging.QueryTicketsAsync();
            Console.WriteLine(JsonConvert.SerializeObject(tickets, Formatting.Indented));


            var ids = new List<long>();

            try
            {
                foreach (var createTicketModel in CreateTickets())
                {
                    var created = await TicketLogging.CreateTicketAsync(createTicketModel);
                    ids.Add(created.Id);
                    Console.WriteLine(JsonConvert.SerializeObject(created, Formatting.Indented));

                    var queried = await TicketLogging.QueryTicketAsync(created.Id);
                    Console.WriteLine(JsonConvert.SerializeObject(queried, Formatting.Indented));

                    await TicketLogging.AttachFileToTicketAsync(queried.Id, CreateAttachments());
                }

                while (ids.Any())
                {
                    await TicketLogging.DeleteTicketAsync(ids[0]);
                    ids.RemoveAt(0);
                }
            }
            catch
            {
                // if something failed, try to delete all created tickets.
                foreach (var id in ids)
                {
                    try { await TicketLogging.DeleteTicketAsync(id); } catch { /* ignored */ }
                }

                throw;
            }
        }
    }
}