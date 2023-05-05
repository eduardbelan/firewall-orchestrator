﻿using NUnit.Framework;
using FWO.Logging;
using FWO.Report;
using FWO.Report.Filter;
using FWO.Api.Data;


namespace FWO.Test
{
    [TestFixture]
    internal class ExportTest
    {
        static NetworkObject TestIp1 = new NetworkObject(){ Id = 1, Name = "TestIp1", IP = "1.2.3.4/32", IpEnd = "", Type = new NetworkObjectType(){ Name = "network" }};
        static NetworkObject TestIp2 = new NetworkObject(){ Id = 2, Name = "TestIp2", IP = "127.0.0.1/32", IpEnd = "", Type = new NetworkObjectType(){ Name = "network" }};
        static NetworkObject TestIpRange = new NetworkObject(){ Id = 3, Name = "TestIpRange", IP = "1.2.3.4/32", IpEnd = "1.2.3.5/32", Type = new NetworkObjectType(){ Name = "ip_range" }};
        static NetworkObject TestIpNew = new NetworkObject(){ Id = 4, Name = "TestIpNew", IP = "10.0.6.1/32", Type = new NetworkObjectType(){ Name = "network" }};
        static NetworkObject TestIp1Changed = new NetworkObject(){ Id = 5, Name = "TestIp1Changed", IP = "2.3.4.5/32", IpEnd = "", Type = new NetworkObjectType(){ Name = "network" }};

        static NetworkService TestService1 = new NetworkService(){  Id = 1, DestinationPort = 443, DestinationPortEnd = 443, Name = "TestService1", Protocol = new NetworkProtocol { Name = "TCP" }};
        static NetworkService TestService2 = new NetworkService(){  Id = 2, DestinationPort = 6666, DestinationPortEnd = 7777, Name = "TestService2", Protocol = new NetworkProtocol { Name = "UDP" }};

        static NetworkUser TestUser1 = new NetworkUser(){ Id = 1, Name = "TestUser1" };
        static NetworkUser TestUser2 = new NetworkUser(){ Id = 2, Name = "TestUser2" };

        static Rule Rule1 = new Rule();
        static Rule Rule1Changed = new Rule();
        static Rule Rule2 = new Rule();
        static Rule Rule2Changed = new Rule();
        static Rule NatRule = new Rule();
        static Rule RecertRule1 = new Rule();
        static Rule RecertRule2 = new Rule();

        SimulatedUserConfig userConfig = new SimulatedUserConfig();
        DynGraphqlQuery query = new DynGraphqlQuery("TestFilter"){ ReportTimeString = "2023-04-20T17:50:04" };

        [SetUp]
        public void Initialize()
        {
        }

        [Test]
        public void RulesGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting rules report html generation");
            ReportRules reportRules = new ReportRules(query, userConfig, ReportType.Rules);
            reportRules.Managements = ConstructRuleReport(false);

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Rules Report</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>Rules Report</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Time of configuration: 2023-04-20T15:50:04Z (UTC)</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>No.</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Action</th><th>Track</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestRule1</td><td>srczn</td>" +
            "<td><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"\">TestIp1</a> (1.2.3.4/32)<br><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"\">TestIp2</a> (127.0.0.1/32)</td>" +
            "<td>dstzn</td>" +
            "<td><span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</td>" +
            "<td><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc1\" target=\"_top\" style=\"\">TestService1</a> (443/TCP)</td>" +
            "<td>accept</td><td>none</td><td><b>Y</b></td><td>uid1</td><td>comment1</td></tr>" +
            "<tr><td>2</td><td>TestRule2</td><td></td>" +
            "<td>not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"\">TestIp1</a> (1.2.3.4/32)<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"\">TestIp2</a> (127.0.0.1/32)</td>" +
            "<td></td>" +
            "<td>not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user2\" target=\"_top\" style=\"\">TestUser2</a>@<span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</td>" +
            "<td>not<br><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc2\" target=\"_top\" style=\"\">TestService2</a> (6666-7777/UDP)</td>" +
            "<td>deny</td><td>none</td><td><b>Y</b></td><td>uid2:123</td><td>comment2</td></tr></table>" +
            "<h4>Network Objects</h4><hr>" +
            "<table><tr><th>No.</th><th>Name</th><th>Type</th><th>IP Address</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td><a name=nwobj1>TestIp1</a></td><td>network</td><td>1.2.3.4/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td><a name=nwobj2>TestIp2</a></td><td>network</td><td>127.0.0.1/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>3</td><td><a name=nwobj3>TestIpRange</a></td><td>ip_range</td><td>1.2.3.4/32-1.2.3.5/32</td><td></td><td></td><td></td></tr>" +
            "</table>" +
            "<h4>Network Services</h4><hr>" +
            "<table><tr><th>No.</th><th>Name</th><th>Type</th><th>Protocol</th><th>Port</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestService1</td><td><a name=svc1>TestService1</a></td><td>TCP</td><td>443</td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td>TestService2</td><td><a name=svc2>TestService2</a></td><td>UDP</td><td>6666-7777</td><td></td><td></td><td></td></tr>" +
            "</table>" +
            "<h4>Users</h4><hr>" +
            "<table><tr><th>No.</th><th>Name</th><th>Type</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestUser1</td><td><a name=user1>TestUser1</a></td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td>TestUser2</td><td><a name=user2>TestUser2</a></td><td></td><td></td><td></td></tr>" +
            "</table></body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportRules.ExportToHtml(), true))));
        }

        [Test]
        public void ResolvedRulesGenerateCsv()
        {
            Log.WriteInfo("Test Log", "starting rules report resolved csv generation");
            ReportRules reportRules = new ReportRules(query, userConfig, ReportType.ResolvedRules);
            reportRules.Managements = ConstructRuleReport(true);

            string expectedCsvResult = "# report type: Rules Report (resolved)" +
            "# report generation date: Z (UTC)" +
            "# date of configuration shown: 2023-04-20T15:50:04Z (UTC)" +
            "# device filter: TestMgt [TestDev]" +
            "# other filters: TestFilter" +
            "# report generator: Firewall Orchestrator - https://fwo.cactus.de/en" +
            "# data protection level: For internal use only#" +
            "\"management-name\",\"device-name\",\"rule-number\",\"rule-name\",\"source-zone\",\"source\",\"destination-zone\",\"destination\",\"service\",\"action\",\"track\",\"rule-enabled\",\"rule-uid\",\"rule-comment\"" +
            "\"TestMgt\",\"TestDev\",\"1\",\"TestRule1\",\"srczn\",\"TestIp1 (1.2.3.4/32),TestIp2 (127.0.0.1/32)\",\"dstzn\",\"TestIpRange (1.2.3.4/32-1.2.3.5/32)\",\"TestService1 (443/TCP)\",\"accept\",\"none\",\"enabled\",\"uid1\",\"comment1\"" +
            "\"TestMgt\",\"TestDev\",\"2\",\"TestRule2\",\"\",\"not(TestUser1@TestIp1 (1.2.3.4/32),TestUser1@TestIp2 (127.0.0.1/32))\",\"\",\"not(TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32))\",\"not(TestService2 (6666-7777/UDP))\",\"deny\",\"none\",\"enabled\",\"uid2:123\",\"comment2\"";
            Assert.AreEqual(expectedCsvResult, removeLinebreaks(removeGenDate(reportRules.ExportToCsv())));
        }

        [Test]
        public void ResolvedRulesGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting rules report resolved html generation");
            ReportRules reportRules = new ReportRules(query, userConfig, ReportType.ResolvedRules);
            reportRules.Managements = ConstructRuleReport(true);

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Rules Report (resolved)</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>Rules Report (resolved)</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Time of configuration: 2023-04-20T15:50:04Z (UTC)</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>No.</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Action</th><th>Track</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestRule1</td><td>srczn</td>" +
            "<td>TestIp1 (1.2.3.4/32)<br>TestIp2 (127.0.0.1/32)</td>" +
            "<td>dstzn</td>" +
            "<td>TestIpRange (1.2.3.4/32-1.2.3.5/32)</td>" +
            "<td>TestService1 (443/TCP)</td>" +
            "<td>accept</td><td>none</td><td><b>Y</b></td><td>uid1</td><td>comment1</td></tr>" +
            "<tr><td>2</td><td>TestRule2</td><td></td>" +
            "<td>not<br>TestUser1@TestIp1 (1.2.3.4/32)<br>TestUser1@TestIp2 (127.0.0.1/32)</td>" +
            "<td></td>" +
            "<td>not<br>TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32)</td>" +
            "<td>not<br>TestService2 (6666-7777/UDP)</td>" +
            "<td>deny</td><td>none</td><td><b>Y</b></td><td>uid2:123</td><td>comment2</td></tr></table>" +
            "</body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportRules.ExportToHtml(), true))));
        }

        [Test]
        public void ResolvedRulesTechGenerateCsv()
        {
            Log.WriteInfo("Test Log", "starting rules report tech csv generation");
            ReportRules reportRules = new ReportRules(query, userConfig, ReportType.ResolvedRulesTech);
            reportRules.Managements = ConstructRuleReport(true);

            string expectedCsvResult = "# report type: Rules Report (technical)" +
            "# report generation date: Z (UTC)" +
            "# date of configuration shown: 2023-04-20T15:50:04Z (UTC)" +
            "# device filter: TestMgt [TestDev]" +
            "# other filters: TestFilter" +
            "# report generator: Firewall Orchestrator - https://fwo.cactus.de/en" +
            "# data protection level: For internal use only#" +
            "\"management-name\",\"device-name\",\"rule-number\",\"rule-name\",\"source-zone\",\"source\",\"destination-zone\",\"destination\",\"service\",\"action\",\"track\",\"rule-enabled\",\"rule-uid\",\"rule-comment\"" +
            "\"TestMgt\",\"TestDev\",\"1\",\"TestRule1\",\"srczn\",\"1.2.3.4/32,127.0.0.1/32\",\"dstzn\",\"1.2.3.4/32-1.2.3.5/32\",\"443/TCP\",\"accept\",\"none\",\"enabled\",\"uid1\",\"comment1\"" +
            "\"TestMgt\",\"TestDev\",\"2\",\"TestRule2\",\"\",\"not(TestUser1@1.2.3.4/32,TestUser1@127.0.0.1/32)\",\"\",\"not(TestUser2@1.2.3.4/32-1.2.3.5/32)\",\"not(6666-7777/UDP)\",\"deny\",\"none\",\"enabled\",\"uid2:123\",\"comment2\"";
            Assert.AreEqual(expectedCsvResult, removeLinebreaks(removeGenDate(reportRules.ExportToCsv())));
        }

        [Test]
        public void ResolvedRulesTechGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting rules report resolved html generation");
            ReportRules reportRules = new ReportRules(query, userConfig, ReportType.ResolvedRulesTech);
            reportRules.Managements = ConstructRuleReport(true);

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Rules Report (technical)</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>Rules Report (technical)</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Time of configuration: 2023-04-20T15:50:04Z (UTC)</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>No.</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Action</th><th>Track</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestRule1</td><td>srczn</td>" +
            "<td>1.2.3.4/32<br>127.0.0.1/32</td>" +
            "<td>dstzn</td>" +
            "<td>1.2.3.4/32-1.2.3.5/32</td>" +
            "<td>443/TCP</td>" +
            "<td>accept</td><td>none</td><td><b>Y</b></td><td>uid1</td><td>comment1</td></tr>" +
            "<tr><td>2</td><td>TestRule2</td><td></td>" +
            "<td>not<br>TestUser1@1.2.3.4/32<br>TestUser1@127.0.0.1/32</td>" +
            "<td></td>" +
            "<td>not<br>TestUser2@1.2.3.4/32-1.2.3.5/32</td>" +
            "<td>not<br>6666-7777/UDP</td>" +
            "<td>deny</td><td>none</td><td><b>Y</b></td><td>uid2:123</td><td>comment2</td></tr></table>" +
            "</body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportRules.ExportToHtml(), true))));
        }

        [Test]
        public void RecertReportGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting recert report html generation");
            ReportRules reportRecerts = new ReportRules(query, userConfig, ReportType.Recertification);
            reportRecerts.Managements = ConstructRecertReport();

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Recertification Report</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>Recertification Report</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Time of configuration: 2023-04-20T15:50:04Z (UTC)</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>No.</th><th>Next Recertification Date</th><th>Owner</th><th>IP address match</th><th>Last Hit</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Action</th><th>Track</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td>" +
            $"<td><p>1.&nbsp;{DateOnly.FromDateTime(DateTime.Now.AddDays(5)).ToString("yyyy-MM-dd")}</p><p style=\"color: red;\">2.&nbsp;{DateOnly.FromDateTime(DateTime.Now.AddDays(-5)).ToString("yyyy-MM-dd")}</p></td>" +
            "<td><p>1.&nbsp;TestOwner1</p><p>2.&nbsp;TestOwner2</p></td>" +
            "<td><p>1.&nbsp;TestIp1</p><p>2.&nbsp;TestIp2</p></td>" +
            "<td></td>" +
            "<td>TestRule1</td>" +
            "<td>srczn</td>" +
            "<td><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"\">TestIp1</a> (1.2.3.4/32)<br><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"\">TestIp2</a> (127.0.0.1/32)</td>" +
            "<td>dstzn</td>" +
            "<td><span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</td>" +
            "<td><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc1\" target=\"_top\" style=\"\">TestService1</a> (443/TCP)</td>" +
            "<td>accept</td>" +
            "<td>none</td>" +
            "<td><b>Y</b></td>" +
            "<td>uid1</td>" +
            "<td>comment1</td></tr>" +
            "<tr><td>2</td>" +
            $"<td><p style=\"color: red;\">{DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd")}</p></td>" +
            "<td><p>TestOwner1</p></td>" +
            "<td><p>TestIpRange</p></td>" +
            "<td></td>" +
            "<td>TestRule2</td>" +
            "<td></td>" +
            "<td>not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"\">TestIp1</a> (1.2.3.4/32)<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"\">TestIp2</a> (127.0.0.1/32)</td>" +
            "<td></td>" +
            "<td>not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user2\" target=\"_top\" style=\"\">TestUser2</a>@<span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</td>" +
            "<td>not<br><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc2\" target=\"_top\" style=\"\">TestService2</a> (6666-7777/UDP)</td>" +
            "<td>deny</td>" +
            "<td>none</td>" +
            "<td><b>Y</b></td>" +
            "<td>uid2:123</td>" +
            "<td>comment2</td></tr></table>" +
            "<h4>Network Objects</h4><hr><table><tr><th>No.</th><th>Name</th><th>Type</th><th>IP Address</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td><a name=nwobj1>TestIp1</a></td><td>network</td><td>1.2.3.4/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td><a name=nwobj2>TestIp2</a></td><td>network</td><td>127.0.0.1/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>3</td><td><a name=nwobj3>TestIpRange</a></td><td>ip_range</td><td>1.2.3.4/32-1.2.3.5/32</td><td></td><td></td><td></td></tr>" +
            "</table>" +
            "<h4>Network Services</h4><hr><table><tr><th>No.</th><th>Name</th><th>Type</th><th>Protocol</th><th>Port</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestService1</td><td><a name=svc1>TestService1</a></td><td>TCP</td><td>443</td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td>TestService2</td><td><a name=svc2>TestService2</a></td><td>UDP</td><td>6666-7777</td><td></td><td></td><td></td></tr>" +
            "</table>" +
            "<h4>Users</h4><hr><table><tr><th>No.</th><th>Name</th><th>Type</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestUser1</td><td><a name=user1>TestUser1</a></td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td>TestUser2</td><td><a name=user2>TestUser2</a></td><td></td><td></td><td></td></tr>" +
            "</table></body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportRecerts.ExportToHtml(), true))));
        }

        [Test]
        public void NatRulesGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting nat rules report html generation");
            ReportNatRules reportNatRules = new ReportNatRules(query, userConfig, ReportType.NatRules);
            reportNatRules.Managements = ConstructNatRuleReport();

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>NAT Rules Report</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>NAT Rules Report</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Time of configuration: 2023-04-20T15:50:04Z (UTC)</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>No.</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Translated Source</th><th>Translated Destination</th><th>Translated Services</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td>" +
            "<td>TestRule1</td>" +
            "<td>srczn</td>" +
            "<td><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"\">TestIp1</a> (1.2.3.4/32)<br><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"\">TestIp2</a> (127.0.0.1/32)</td>" +
            "<td>dstzn</td>" +
            "<td><span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</td>" +
            "<td><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc1\" target=\"_top\" style=\"\">TestService1</a> (443/TCP)</td>" +
            "<td><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user2\" target=\"_top\" style=\"\">TestUser2</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj5\" target=\"_top\" style=\"\">TestIp1Changed</a> (2.3.4.5/32)</td>" +
            "<td>not<br><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj5\" target=\"_top\" style=\"\">TestIp1Changed</a> (2.3.4.5/32)<br><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj4\" target=\"_top\" style=\"\">TestIpNew</a> (10.0.6.1/32)</td>" +
            "<td><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc1\" target=\"_top\" style=\"\">TestService1</a> (443/TCP)<br><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc2\" target=\"_top\" style=\"\">TestService2</a> (6666-7777/UDP)</td>" +
            "<td><b>Y</b></td>" +
            "<td>uid1</td>" +
            "<td>comment1</td></tr></table>" +
            "<h4>Network Objects</h4><hr><table><tr><th>No.</th><th>Name</th><th>Type</th><th>IP Address</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td><a name=nwobj1>TestIp1</a></td><td>network</td><td>1.2.3.4/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td><a name=nwobj2>TestIp2</a></td><td>network</td><td>127.0.0.1/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>3</td><td><a name=nwobj3>TestIpRange</a></td><td>ip_range</td><td>1.2.3.4/32-1.2.3.5/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>4</td><td><a name=nwobj4>TestIpNew</a></td><td>network</td><td>10.0.6.1/32</td><td></td><td></td><td></td></tr>" +
            "<tr><td>5</td><td><a name=nwobj5>TestIp1Changed</a></td><td>network</td><td>2.3.4.5/32</td><td></td><td></td><td></td></tr>" +
            "</table>" +
            "<h4>Network Services</h4><hr><table><tr><th>No.</th><th>Name</th><th>Type</th><th>Protocol</th><th>Port</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestService1</td><td><a name=svc1>TestService1</a></td><td>TCP</td><td>443</td><td></td><td></td><td></td></tr>" +
            "<tr><td>2</td><td>TestService2</td><td><a name=svc2>TestService2</a></td><td>UDP</td><td>6666-7777</td><td></td><td></td><td></td></tr>" +
            "</table>" +
            "<h4>Users</h4><hr><table><tr><th>No.</th><th>Name</th><th>Type</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>1</td><td>TestUser2</td><td><a name=user2>TestUser2</a></td><td></td><td></td><td></td></tr>" +
            "</table></table></body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportNatRules.ExportToHtml(), true))));
        }

        [Test]
        public void ChangesGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting changes report html generation");
            ReportChanges reportChanges = new ReportChanges(query, userConfig, ReportType.Changes);
            reportChanges.Managements = ConstructChangeReport(false);

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Changes Report</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>Changes Report</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>Change Time</th><th>Change Type</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Action</th><th>Track</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule added</td><td><p style=\"color: green; text-decoration: bold;\">TestRule1</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">srczn</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\"><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestIp1</a> (1.2.3.4/32)<br><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestIp2</a> (127.0.0.1/32)</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">dstzn</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\"><span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\"><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc1\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestService1</a> (443/TCP)</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">accept</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">none</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\"><b>Y</b></p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">uid1</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">comment1</p></td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule modified</td><td>TestRule1</td><td>srczn</td>" +
            "<td><p><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"\">TestIp2</a> (127.0.0.1/32)<br></p>" +
            "deleted: <p style=\"color: red; text-decoration: line-through red;\"><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestIp1</a> (1.2.3.4/32)<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\"><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj5\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestIp1Changed</a> (2.3.4.5/32)</p></td>" +
            "<td>dstzn</td>" +
            "<td><p><span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\"><span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj4\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestIpNew</a> (10.0.6.1/32)</p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\"><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc1\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestService1</a> (443/TCP)<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\">not<br><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc1\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestService1</a> (443/TCP)</p></td>" +
            "<td>accept</td><td>none</td><td><b>Y</b></td><td>deleted: <p style=\"color: red; text-decoration: line-through red;\">uid1<br></p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">comment1<br></p>added: <p style=\"color: green; text-decoration: bold;\">new comment</p></td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule modified</td><td>TestRule2</td><td></td>" +
            "<td>not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"\">TestIp1</a> (1.2.3.4/32)<br>" +
            "<span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"\">TestIp2</a> (127.0.0.1/32)</td>" +
            "<td></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user2\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestUser2</a>@<span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\"><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user2\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestUser2</a>@<span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">not<br><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc2\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestService2</a> (6666-7777/UDP)<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\"><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc2\" target=\"_top\" style=\"color: green; text-decoration: bold;\">TestService2</a> (6666-7777/UDP)</p></td>" +
            "<td>deny</td><td>none</td><td>deleted: <p style=\"color: red; text-decoration: line-through red;\"><b>Y</b><br></p>added: <p style=\"color: green; text-decoration: bold;\"><b>N</b></p></td><td>uid2:123</td><td>comment2</td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule deleted</td><td><p style=\"color: red; text-decoration: line-through red;\">TestRule2</p></td><td></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj1\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestIp1</a> (1.2.3.4/32)<br>" +
            "<span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user1\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestUser1</a>@<span class=\"oi oi-rss\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj2\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestIp2</a> (127.0.0.1/32)</p></td>" +
            "<td></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br><span class=\"oi oi-people\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#user2\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestUser2</a>@<span class=\"oi oi-resize-width\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#nwobj3\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestIpRange</a> (1.2.3.4/32-1.2.3.5/32)</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br><span class=\"oi oi-wrench\">&nbsp;</span><a @onclick:stopPropagation=\"true\" href=\"#svc2\" target=\"_top\" style=\"color: red; text-decoration: line-through red;\">TestService2</a> (6666-7777/UDP)</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">deny</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">none</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\"><b>Y</b></p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">uid2:123</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">comment2</p></td></tr></table>" +
            // "<h4>Network Objects</h4><hr>" +
            // "<table><tr><th>No.</th><th>Name</th><th>Type</th><th>IP Address</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            // "<tr><td>1</td><td><a name=nwobj1>TestIp1</a></td><td>network</td><td>1.2.3.4/32</td><td></td><td></td><td></td></tr>" +
            // "<tr><td>2</td><td><a name=nwobj2>TestIp2</a></td><td>network</td><td>127.0.0.1/32</td><td></td><td></td><td></td></tr>" +
            // "<tr><td>3</td><td><a name=nwobj3>TestIpRange</a></td><td>ip_range</td><td>1.2.3.4/32-1.2.3.5/32</td><td></td><td></td><td></td></tr>" +
            // "<tr><td>3</td><td><a name=nwobj4>TestIpNew</a></td><td>network</td><td>10.0.6.1/32</td><td></td><td></td><td></td></tr>" +
            // "<tr><td>3</td><td><a name=nwobj5>TestIp1Changed</a></td><td>network</td><td>2.3.4.5/32</td><td></td><td></td><td></td></tr>" +
            // "</table>" +
            // "<h4>Network Services</h4><hr>" +
            // "<table><tr><th>No.</th><th>Name</th><th>Type</th><th>Protocol</th><th>Port</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            // "<tr><td>1</td><td>TestService1</td><td><a name=svc1>TestService1</a></td><td>TCP</td><td>443</td><td></td><td></td><td></td></tr>" +
            // "<tr><td>2</td><td>TestService2</td><td><a name=svc2>TestService2</a></td><td>UDP</td><td>6666-7777</td><td></td><td></td><td></td></tr>" +
            // "</table>" +
            // "<h4>Users</h4><hr>" +
            // "<table><tr><th>No.</th><th>Name</th><th>Type</th><th>Members</th><th>Uid</th><th>Comment</th></tr>" +
            // "<tr><td>1</td><td>TestUser1</td><td><a name=user1>TestUser1</a></td><td></td><td></td><td></td></tr>" +
            // "<tr><td>2</td><td>TestUser2</td><td><a name=user2>TestUser2</a></td><td></td><td></td><td></td></tr>" +
            // "</table>"+
            "</body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportChanges.ExportToHtml(), true))));
        }

        [Test]
        public void ResolvedChangesGenerateCsv()
        {
            Log.WriteInfo("Test Log", "starting changes report resolved csv generation");
            ReportChanges reportChanges = new ReportChanges(query, userConfig, ReportType.ResolvedChanges);
            reportChanges.Managements = ConstructChangeReport(true);

            string expectedCsvResult = "# report type: Changes Report (resolved)" +
            "# report generation date: Z (UTC)" +
            "# device filter: TestMgt [TestDev]" +
            "# other filters: TestFilter" +
            "# report generator: Firewall Orchestrator - https://fwo.cactus.de/en" +
            "# data protection level: For internal use only#" +
            "\"management-name\",\"device-name\",\"change-time\",\"change-type\",\"rule-name\",\"source-zone\",\"source\",\"destination-zone\",\"destination\",\"service\",\"action\",\"track\",\"rule-enabled\",\"rule-uid\",\"rule-comment\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule added\",\"TestRule1\",\"srczn\",\"TestIp1 (1.2.3.4/32),TestIp2 (127.0.0.1/32)\"," +
            "\"dstzn\",\"TestIpRange (1.2.3.4/32-1.2.3.5/32)\",\"TestService1 (443/TCP)\",\"accept\",\"none\",\"enabled\",\"uid1\",\"comment1\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule modified\",\"TestRule1\",\"srczn\",\"TestIp2 (127.0.0.1/32) deleted: TestIp1 (1.2.3.4/32) added: TestIp1Changed (2.3.4.5/32)\"," +
            "\"dstzn\",\"TestIpRange (1.2.3.4/32-1.2.3.5/32) added: TestIpNew (10.0.6.1/32)\"," +
            "\" deleted: TestService1 (443/TCP) added: not(TestService1 (443/TCP))\",\"accept\",\"none\",\"enabled\",\" deleted: uid1\",\" deleted: comment1 added: new comment\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule modified\",\"TestRule2\",\"\",\"not(TestUser1@TestIp1 (1.2.3.4/32),TestUser1@TestIp2 (127.0.0.1/32))\"," +
            "\"\",\" deleted: not(TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32)) added: TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32)\"," +
            "\" deleted: not(TestService2 (6666-7777/UDP)) added: TestService2 (6666-7777/UDP)\",\"deny\",\"none\",\" deleted: enabled added: disabled\",\"uid2:123\",\"comment2\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule deleted\",\"TestRule2\",\"\",\"not(TestUser1@TestIp1 (1.2.3.4/32),TestUser1@TestIp2 (127.0.0.1/32))\"," +
            "\"\",\"not(TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32))\",\"not(TestService2 (6666-7777/UDP))\",\"deny\",\"none\",\"enabled\",\"uid2:123\",\"comment2\"";
            Assert.AreEqual(expectedCsvResult, removeLinebreaks(removeGenDate(reportChanges.ExportToCsv())));
        }

        [Test]
        public void ResolvedChangesGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting changes report resolved html generation");
            ReportChanges reportChanges = new ReportChanges(query, userConfig, ReportType.ResolvedChanges);
            reportChanges.Managements = ConstructChangeReport(true);

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Changes Report (resolved)</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>Changes Report (resolved)</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>Change Time</th><th>Change Type</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Action</th><th>Track</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule added</td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">TestRule1</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">srczn</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">TestIp1 (1.2.3.4/32)<br>TestIp2 (127.0.0.1/32)</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">dstzn</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">TestIpRange (1.2.3.4/32-1.2.3.5/32)</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">TestService1 (443/TCP)</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">accept</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">none</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\"><b>Y</b></p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">uid1</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">comment1</p></td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule modified</td><td>TestRule1</td><td>srczn</td>" +
            "<td><p>TestIp2 (127.0.0.1/32)<br></p>deleted: <p style=\"color: red; text-decoration: line-through red;\">TestIp1 (1.2.3.4/32)<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\">TestIp1Changed (2.3.4.5/32)</p></td>" +
            "<td>dstzn</td>" +
            "<td><p>TestIpRange (1.2.3.4/32-1.2.3.5/32)<br></p>added: <p style=\"color: green; text-decoration: bold;\">TestIpNew (10.0.6.1/32)</p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">TestService1 (443/TCP)<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\">not<br>TestService1 (443/TCP)</p></td>" +
            "<td>accept</td><td>none</td><td><b>Y</b></td><td>deleted: <p style=\"color: red; text-decoration: line-through red;\">uid1<br></p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">comment1<br></p>added: <p style=\"color: green; text-decoration: bold;\">new comment</p></td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule modified</td><td>TestRule2</td><td></td>" +
            "<td>not<br>TestUser1@TestIp1 (1.2.3.4/32)<br>TestUser1@TestIp2 (127.0.0.1/32)</td>" +
            "<td></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">not<br>TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32)<br></p>added: <p style=\"color: green; text-decoration: bold;\">TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32)</p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">not<br>TestService2 (6666-7777/UDP)<br></p>added: <p style=\"color: green; text-decoration: bold;\">TestService2 (6666-7777/UDP)</p></td>" +
            "<td>deny</td><td>none</td><td>deleted: <p style=\"color: red; text-decoration: line-through red;\"><b>Y</b><br></p>added: <p style=\"color: green; text-decoration: bold;\"><b>N</b></p></td><td>uid2:123</td><td>comment2</td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule deleted</td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">TestRule2</p></td><td></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br>TestUser1@TestIp1 (1.2.3.4/32)<br>TestUser1@TestIp2 (127.0.0.1/32)</p></td>" +
            "<td></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br>TestUser2@TestIpRange (1.2.3.4/32-1.2.3.5/32)</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br>TestService2 (6666-7777/UDP)</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">deny</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">none</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\"><b>Y</b></p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">uid2:123</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">comment2</p></td></tr></table>" +
            "</body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportChanges.ExportToHtml(), true))));
        }

        [Test]
        public void ResolvedChangesTechGenerateCsv()
        {
            Log.WriteInfo("Test Log", "starting changes report tech csv generation");
            ReportChanges reportChanges = new ReportChanges(query, userConfig, ReportType.ResolvedChangesTech);
            reportChanges.Managements = ConstructChangeReport(true);

            string expectedCsvResult = "# report type: Changes Report (technical)" +
            "# report generation date: Z (UTC)" +
            "# device filter: TestMgt [TestDev]" +
            "# other filters: TestFilter" +
            "# report generator: Firewall Orchestrator - https://fwo.cactus.de/en" +
            "# data protection level: For internal use only#" +
            "\"management-name\",\"device-name\",\"change-time\",\"change-type\",\"rule-name\",\"source-zone\",\"source\",\"destination-zone\",\"destination\",\"service\",\"action\",\"track\",\"rule-enabled\",\"rule-uid\",\"rule-comment\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule added\",\"TestRule1\",\"srczn\",\"1.2.3.4/32,127.0.0.1/32\",\"dstzn\",\"1.2.3.4/32-1.2.3.5/32\",\"443/TCP\",\"accept\",\"none\",\"enabled\",\"uid1\",\"comment1\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule modified\",\"TestRule1\",\"srczn\",\"127.0.0.1/32 deleted: 1.2.3.4/32 added: 2.3.4.5/32\",\"dstzn\",\"1.2.3.4/32-1.2.3.5/32 added: 10.0.6.1/32\",\" deleted: 443/TCP added: not(443/TCP)\",\"accept\",\"none\",\"enabled\",\" deleted: uid1\",\" deleted: comment1 added: new comment\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule modified\",\"TestRule2\",\"\",\"not(TestUser1@1.2.3.4/32,TestUser1@127.0.0.1/32)\",\"\",\" deleted: not(TestUser2@1.2.3.4/32-1.2.3.5/32) added: TestUser2@1.2.3.4/32-1.2.3.5/32\",\" deleted: not(6666-7777/UDP) added: 6666-7777/UDP\",\"deny\",\"none\",\" deleted: enabled added: disabled\",\"uid2:123\",\"comment2\"" +
            "\"TestMgt\",\"TestDev\",\"05.04.2023 12:00:00\",\"Rule deleted\",\"TestRule2\",\"\",\"not(TestUser1@1.2.3.4/32,TestUser1@127.0.0.1/32)\",\"\",\"not(TestUser2@1.2.3.4/32-1.2.3.5/32)\",\"not(6666-7777/UDP)\",\"deny\",\"none\",\"enabled\",\"uid2:123\",\"comment2\"";
            Assert.AreEqual(expectedCsvResult, removeLinebreaks(removeGenDate(reportChanges.ExportToCsv())));
        }

        [Test]
        public void ResolvedChangesTechGenerateHtml()
        {
            Log.WriteInfo("Test Log", "starting changes report tech html generation");
            ReportChanges reportChanges = new ReportChanges(query, userConfig, ReportType.ResolvedChangesTech);
            reportChanges.Managements = ConstructChangeReport(true);

            string expectedHtmlResult = "<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>Changes Report (technical)</title>" +
            "<style>table {font-family: arial, sans-serif;font-size: 10px;border-collapse: collapse;width: 100 %;}td {border: 1px solid #000000;text-align: left;padding: 3px;}th {border: 1px solid #000000;text-align: left;padding: 3px;background-color: #dddddd;}</style></head>" +
            "<body>" +
            "<h2>Changes Report (technical)</h2>" +
            "<p>Filter: TestFilter</p>" +
            "<p>Generated on: Z (UTC)</p>" +
            "<p>Devices: TestMgt [TestDev]</p><hr>" +
            "<h3>TestMgt</h3><hr>" +
            "<h4>TestDev</h4><hr>" +
            "<table><tr><th>Change Time</th><th>Change Type</th><th>Name</th><th>Source Zone</th><th>Source</th><th>Destination Zone</th><th>Destination</th><th>Services</th><th>Action</th><th>Track</th><th>Enabled</th><th>Uid</th><th>Comment</th></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule added</td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">TestRule1</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">srczn</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">1.2.3.4/32<br>127.0.0.1/32</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">dstzn</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">1.2.3.4/32-1.2.3.5/32</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">443/TCP</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">accept</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">none</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\"><b>Y</b></p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">uid1</p></td>" +
            "<td><p style=\"color: green; text-decoration: bold;\">comment1</p></td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule modified</td><td>TestRule1</td><td>srczn</td>" +
            "<td><p>127.0.0.1/32<br></p>" +
            "deleted: <p style=\"color: red; text-decoration: line-through red;\">1.2.3.4/32<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\">2.3.4.5/32</p></td>" +
            "<td>dstzn</td>" +
            "<td><p>1.2.3.4/32-1.2.3.5/32<br></p>added: <p style=\"color: green; text-decoration: bold;\">10.0.6.1/32</p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">443/TCP<br></p>" +
            "added: <p style=\"color: green; text-decoration: bold;\">not<br>443/TCP</p></td>" +
            "<td>accept</td><td>none</td><td><b>Y</b></td><td>deleted: <p style=\"color: red; text-decoration: line-through red;\">uid1<br></p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">comment1<br></p>added: <p style=\"color: green; text-decoration: bold;\">new comment</p></td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule modified</td><td>TestRule2</td><td></td>" +
            "<td>not<br>TestUser1@1.2.3.4/32<br>TestUser1@127.0.0.1/32</td>" +
            "<td></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">not<br>TestUser2@1.2.3.4/32-1.2.3.5/32<br></p>added: <p style=\"color: green; text-decoration: bold;\">TestUser2@1.2.3.4/32-1.2.3.5/32</p></td>" +
            "<td>deleted: <p style=\"color: red; text-decoration: line-through red;\">not<br>6666-7777/UDP<br></p>added: <p style=\"color: green; text-decoration: bold;\">6666-7777/UDP</p></td>" +
            "<td>deny</td><td>none</td><td>deleted: <p style=\"color: red; text-decoration: line-through red;\"><b>Y</b><br></p>added: <p style=\"color: green; text-decoration: bold;\"><b>N</b></p></td><td>uid2:123</td><td>comment2</td></tr>" +
            "<tr><td>05.04.2023 12:00:00</td><td>Rule deleted</td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">TestRule2</p></td><td></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br>TestUser1@1.2.3.4/32<br>TestUser1@127.0.0.1/32</p></td>" +
            "<td></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br>TestUser2@1.2.3.4/32-1.2.3.5/32</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">not<br>6666-7777/UDP</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">deny</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">none</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\"><b>Y</b></p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">uid2:123</p></td>" +
            "<td><p style=\"color: red; text-decoration: line-through red;\">comment2</p></td></tr></table>" +
            "</body></html>";
            Assert.AreEqual(expectedHtmlResult, removeLinebreaks((removeGenDate(reportChanges.ExportToHtml(), true))));
        }


        private NetworkLocation[] InitFroms(bool resolved, bool user = false)
        {
            if(resolved)
            {
                return new NetworkLocation[]{ new NetworkLocation(user ? TestUser1 : new NetworkUser(), new NetworkObject(){ ObjectGroupFlats = new GroupFlat<NetworkObject>[]
                {
                    new GroupFlat<NetworkObject>(){ Object = TestIp1 },
                    new GroupFlat<NetworkObject>(){ Object = TestIp2 }
                }})};
            }
            else
            {
                return new NetworkLocation[]
                {
                    new NetworkLocation(user ? TestUser1 : new NetworkUser(), TestIp1),
                    new NetworkLocation(user ? TestUser1 : new NetworkUser(), TestIp2)
                };
            }
        }

        private NetworkLocation[] InitTos(bool resolved, bool user = false)
        {
            if(resolved)
            {
                return new NetworkLocation[]{ new NetworkLocation(user ? TestUser2 : new NetworkUser(), new NetworkObject(){ ObjectGroupFlats = new GroupFlat<NetworkObject>[]
                {
                    new GroupFlat<NetworkObject>(){ Object = TestIpRange }
                }})};
            }
            else
            {
                return new NetworkLocation[]
                {
                    new NetworkLocation(user ? TestUser2 : new NetworkUser(), TestIpRange),
                };
            }
        }

        private ServiceWrapper[] InitServices(NetworkService service, bool resolved)
        {
            if(resolved)
            {
                return new ServiceWrapper[]{new ServiceWrapper(){ Content = new NetworkService(){ServiceGroupFlats = new GroupFlat<NetworkService>[]
                {
                    new GroupFlat<NetworkService>(){ Object = service }
                }}}};
            }
            else
            {
                return new ServiceWrapper[]
                {
                    new ServiceWrapper(){ Content = service },
                };
            }
        }

        private Rule InitRule1(bool resolved)
        {
            return new Rule()
            {
                Name = "TestRule1",
                Action = "accept",
                Comment = "comment1",
                Disabled = false,
                DisplayOrderNumber = 1,
                Track = "none",
                Uid = "uid1",
                SourceZone = new NetworkZone(){ Name = "srczn" },
                SourceNegated = false,
                Froms = InitFroms(resolved),
                DestinationZone = new NetworkZone(){ Name = "dstzn" },
                DestinationNegated = false,
                Tos = InitTos(resolved),
                ServiceNegated = false,
                Services = InitServices(TestService1, resolved)
            };
        }

        private Rule InitRule2(bool resolved)
        {
            return new Rule()
            {
                Name = "TestRule2",
                Action = "deny",
                Comment = "comment2",
                Disabled = false,
                DisplayOrderNumber = 2,
                Track = "none",
                Uid = "uid2:123",
                SourceNegated = true,
                Froms = InitFroms(resolved, true),
                DestinationNegated = true,
                Tos = InitTos(resolved, true),
                ServiceNegated = true,
                Services = InitServices(TestService2, resolved)
            };
        }

        private Management[] ConstructRuleReport(bool resolved)
        {
            Rule1 = InitRule1(resolved);
            Rule2 = InitRule2(resolved);
            return new List<Management>()
            {
                new Management()
                { 
                    Name = "TestMgt",
                    ReportObjects = new NetworkObject[]{ TestIp1, TestIp2, TestIpRange },
                    ReportServices = new NetworkService[]{ TestService1, TestService2 },
                    ReportUsers = new NetworkUser[]{ TestUser1, TestUser2 },
                    Devices = new Device[]
                    { 
                        new Device()
                        { 
                            Name = "TestDev",
                            Rules = new Rule[]{ Rule1, Rule2 }
                        } 
                    }
                }
            }.ToArray();
        }

        private Management[] ConstructRecertReport()
        {
            RecertRule1 = InitRule1(false);
            RecertRule1.Metadata.RuleRecertification = new List<Recertification>()
            {
                new Recertification()
                {
                    NextRecertDate  = DateTime.Now.AddDays(5),
                    FwoOwner = new FwoOwner(){ Name = "TestOwner1" },
                    IpMatch = TestIp1.Name
                },
                new Recertification()
                {
                    NextRecertDate  = DateTime.Now.AddDays(-5),
                    FwoOwner = new FwoOwner(){ Name = "TestOwner2" },
                    IpMatch = TestIp2.Name
                }
            };
            RecertRule2 = InitRule2(false);
            RecertRule2.Metadata.RuleRecertification = new List<Recertification>()
            {
                new Recertification()
                {
                    NextRecertDate  = DateTime.Now,
                    FwoOwner = new FwoOwner(){ Name = "TestOwner1" },
                    IpMatch = TestIpRange.Name
                }
            };
            return new List<Management>()
            {
                new Management()
                { 
                    Name = "TestMgt",
                    ReportObjects = new NetworkObject[]{ TestIp1, TestIp2, TestIpRange },
                    ReportServices = new NetworkService[]{ TestService1, TestService2 },
                    ReportUsers = new NetworkUser[]{ TestUser1, TestUser2 },
                    Devices = new Device[]
                    { 
                        new Device()
                        { 
                            Name = "TestDev", 
                            Rules = new Rule[]{ RecertRule1, RecertRule2 }
                        } 
                    }
                }
            }.ToArray();
        }

        private Management[] ConstructNatRuleReport()
        {
            NatRule = InitRule1(false);
            NatRule.NatData = new NatData()
            {
                TranslatedSourceNegated = false,
                TranslatedFroms = new NetworkLocation[]
                {
                    new NetworkLocation(TestUser2, TestIp1Changed)
                },
                TranslatedDestinationNegated = true,
                TranslatedTos = new NetworkLocation[]
                {
                    new NetworkLocation(new NetworkUser(), TestIp1Changed),
                    new NetworkLocation(new NetworkUser(), TestIpNew)
                },
                TranslatedServiceNegated = false,
                TranslatedServices = new ServiceWrapper[]
                {
                    new ServiceWrapper(){ Content = TestService1 },
                    new ServiceWrapper(){ Content = TestService2 }
                }
            };
            return new List<Management>()
            {
                new Management()
                { 
                    Name = "TestMgt",
                    ReportObjects = new NetworkObject[]{ TestIp1, TestIp2, TestIpRange, TestIpNew, TestIp1Changed },
                    ReportServices = new NetworkService[]{ TestService1, TestService2 },
                    ReportUsers = new NetworkUser[]{ TestUser2 },
                    Devices = new Device[]
                    { 
                        new Device(){ Name = "TestDev", Rules = new Rule[]{ NatRule }} 
                    }
                }
            }.ToArray();
        }

        private Management[] ConstructChangeReport(bool resolved)
        {
            Rule1 = InitRule1(resolved);
            Rule1Changed = InitRule1(resolved);
            Rule2 = InitRule2(resolved);
            Rule2Changed = InitRule2(resolved);
            if(resolved)
            {
                Rule1Changed.Froms[0].Object.ObjectGroupFlats[0].Object = TestIp1Changed;
                Rule1Changed.Tos = new NetworkLocation[]{new NetworkLocation(new NetworkUser(), new NetworkObject(){ObjectGroupFlats = new GroupFlat<NetworkObject>[]
                {
                    new GroupFlat<NetworkObject>(){ Object = TestIpRange },
                    new GroupFlat<NetworkObject>(){ Object = TestIpNew }
                }})};  
            }
            else
            {
                Rule1Changed.Froms[0].Object = TestIp1Changed;
                Rule1Changed.Tos = new NetworkLocation[]
                {
                    new NetworkLocation(new NetworkUser(), TestIpRange),
                    new NetworkLocation(new NetworkUser(), TestIpNew)
                };
            }
            Rule1Changed.Uid = "";
            Rule1Changed.ServiceNegated = true;
            Rule1Changed.Comment = "new comment";

            Rule2Changed.DestinationNegated = false;
            Rule2Changed.ServiceNegated = false;
            Rule2Changed.Disabled = true;

            RuleChange ruleChange1 = new RuleChange()
            {
                ChangeAction = 'I',
                ChangeImport = new ChangeImport(){ Time = new DateTime(2023,04,05,12,0,0) },
                NewRule = Rule1
            };
            RuleChange ruleChange2 = new RuleChange()
            {
                ChangeAction = 'C',
                ChangeImport = new ChangeImport(){ Time = new DateTime(2023,04,05,12,0,0) },
                OldRule = Rule1,
                NewRule = Rule1Changed
            };
            RuleChange ruleChange3 = new RuleChange()
            {
                ChangeAction = 'C',
                ChangeImport = new ChangeImport(){ Time = new DateTime(2023,04,05,12,0,0) },
                OldRule = Rule2,
                NewRule = Rule2Changed
            };
            RuleChange ruleChange4 = new RuleChange()
            {
                ChangeAction = 'D',
                ChangeImport = new ChangeImport(){ Time = new DateTime(2023,04,05,12,0,0) },
                OldRule = Rule2
            };
            return new List<Management>()
            {
                new Management()
                { 
                    Name = "TestMgt",
                    Devices = new Device[]
                    {
                        new Device()
                        { 
                            Name = "TestDev",
                            RuleChanges = new RuleChange[]{ ruleChange1, ruleChange2, ruleChange3, ruleChange4 }
                        }
                    }
                }
            }.ToArray();
        }

        private string removeGenDate(string exportString, bool html = false)
        {
            string dateText = html ? "<p>Generated on: " : "# report generation date: ";
            int startGenTime = exportString.IndexOf(dateText);
            if(startGenTime > 0)
            {
                return exportString.Remove(startGenTime + dateText.Length, 19);
            }
            return exportString;
        }

        private string removeLinebreaks(string exportString)
        {
            while(exportString.Contains("\n "))
            {
                exportString = exportString.Replace("\n ","\n");
            }
            while(exportString.Contains(" \n"))
            {
                exportString = exportString.Replace(" \n","\n");
            }
            while(exportString.Contains(" \r"))
            {
                exportString = exportString.Replace(" \r","\r");
            }
            exportString = exportString.Replace("\r","");
            return exportString.Replace("\n","");
        }
    }
}