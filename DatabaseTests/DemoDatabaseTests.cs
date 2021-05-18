using Database;
using NUnit.Framework;
using Services.Design;
using System.Threading.Tasks;

namespace DatabaseTests
{
    public class DemoDatabaseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task CanSaveAndLoadDemoHeaderData()
        {
            var designService = new DemosDesignService();
            var demos = await designService.GetDemosHeader(null);
            var testDemo = demos[0];
            
            var repository = new SqLiteDemoRepository();
            // Keep tables alive.
            repository.Init();
            repository.SaveDemo(testDemo);

            var loadedDemo = repository.GetDemo(testDemo.Id);

            Assert.AreEqual(testDemo.Id, loadedDemo.Id);
            Assert.AreEqual(testDemo.Comment, loadedDemo.Comment);
            Assert.AreEqual(testDemo.MapName, loadedDemo.MapName);

            Assert.AreEqual(testDemo.Date, loadedDemo.Date);
            Assert.AreEqual(testDemo.SourceName, loadedDemo.SourceName);
            Assert.AreEqual(testDemo.Source?.Name, loadedDemo.Source?.Name);
            Assert.AreEqual(testDemo.ClientName, loadedDemo.ClientName);
            Assert.AreEqual(testDemo.Hostname, loadedDemo.Hostname);
            Assert.AreEqual(testDemo.Type, loadedDemo.Type);
            Assert.AreEqual(testDemo.Tickrate, loadedDemo.Tickrate);
            Assert.AreEqual(testDemo.ServerTickrate, loadedDemo.ServerTickrate);
            Assert.AreEqual(testDemo.Duration, loadedDemo.Duration);
            Assert.AreEqual(testDemo.Ticks, loadedDemo.Ticks);
            Assert.AreEqual(testDemo.MapName, loadedDemo.MapName);
            Assert.AreEqual(testDemo.Path, loadedDemo.Path);
            Assert.AreEqual(testDemo.Comment, loadedDemo.Comment);
            Assert.AreEqual(testDemo.Status, loadedDemo.Status);
            Assert.AreEqual(testDemo.TeamCT.Name, loadedDemo.TeamCT.Name);
            Assert.AreEqual(testDemo.TeamCT.ScoreFirstHalf, loadedDemo.TeamCT.ScoreFirstHalf);
            Assert.AreEqual(testDemo.TeamCT.ScoreSecondHalf, loadedDemo.TeamCT.ScoreSecondHalf);
            Assert.AreEqual(testDemo.TeamT.Name, loadedDemo.TeamT.Name);
            Assert.AreEqual(testDemo.TeamT.ScoreFirstHalf, loadedDemo.TeamT.ScoreFirstHalf);
            Assert.AreEqual(testDemo.TeamT.ScoreSecondHalf, loadedDemo.TeamT.ScoreSecondHalf);
        }
    }
}