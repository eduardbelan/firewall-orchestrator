﻿using NetTools;
using FWO.Config.Api;
using FWO.Basics;
using FWO.Api.Data;
using FWO.Api.Client;
using FWO.Api.Client.Queries;


namespace FWO.Services
{
    public class ModellingAppServerHandler : ModellingHandlerBase
    {
        public ModellingAppServer ActAppServer { get; set; } = new();
        private ModellingAppServer ActAppServerOrig { get; set; } = new();


        public ModellingAppServerHandler(ApiConnection apiConnection, UserConfig userConfig, FwoOwner application,
            ModellingAppServer appServer, List<ModellingAppServer> availableAppServers, bool addMode, Action<Exception?, string, string, bool> displayMessageInUi)
            : base(apiConnection, userConfig, application, addMode, displayMessageInUi)
        {
            ActAppServer = appServer;
            AvailableAppServers = availableAppServers;
            ActAppServerOrig = new ModellingAppServer(ActAppServer);
        }

        public async Task<bool> Save()
        {
            try
            {
                //if (ActAppServer.Sanitize())
                //{
                //    DisplayMessageInUi(null, userConfig.GetText("save_app_server"), userConfig.GetText("U0001"), true);
                //}
                if (CheckAppServer())
                {
                    bool saveOk = true;
                    apiConnection.SetRole(Roles.Admin);
                    if (AddMode)
                    {
                        saveOk &= await AddAppServerToDb();
                    }
                    else
                    {
                        saveOk &= await UpdateAppServerInDb();
                    }
                    apiConnection.SwitchBack();
                    return saveOk;
                }
            }
            catch (Exception exception)
            {
                DisplayMessageInUi(exception, userConfig.GetText("edit_app_server"), "", true);
            }
            return false;
        }

        public void Reset()
        {
            ActAppServer = ActAppServerOrig;
            if (!AddMode && !ReadOnly)
            {
                AvailableAppServers[AvailableAppServers.FindIndex(x => x.Id == ActAppServer.Id)] = ActAppServerOrig;
            }
        }

        private bool CheckAppServer()
        {
            if (ActAppServer.Ip.TryGetNetmask(out string netmask))
            {
                (string Start, string End) = ActAppServer.Ip.CidrToRangeString();
                ActAppServer.Ip = Start;
                ActAppServer.IpEnd = End;
            }
            else if (ActAppServer.Ip.TrySplit('-', 1, out string ipEnd) && IPAddressRange.TryParse(ActAppServer.Ip, out IPAddressRange ipRange))
            {
                ActAppServer.Ip = ipRange.Begin.ToString();
                ActAppServer.IpEnd = ipRange.End.ToString();
            }
            else
            {
                ActAppServer.IpEnd = ActAppServer.Ip;
            }

            if (ActAppServer.Ip == null || ActAppServer.Ip == "" || ActAppServer.CustomType == null || ActAppServer.CustomType == 0)
            {
                DisplayMessageInUi(null, userConfig.GetText("edit_app_server"), userConfig.GetText("E5102"), true);
                return false;
            }
            if (!CheckIpAdress(ActAppServer.Ip))
            {
                DisplayMessageInUi(null, userConfig.GetText("edit_app_server"), userConfig.GetText("wrong_ip_address"), true);
                return false;
            }
            return true;
        }

        private static bool CheckIpAdress(string ip)
        {
            return IPAddressRange.TryParse(ip, out _);
        }

        private async Task<bool> AddAppServerToDb()
        {
            try
            {
                var Variables = new
                {
                    name = ActAppServer.Name,
                    appId = Application.Id,
                    ip = ActAppServer.Ip,
                    ipEnd = ActAppServer.IpEnd,
                    importSource = GlobalConst.kManual,  // todo
                    customType = ActAppServer.CustomType
                };
                ReturnId[]? returnIds = ( await apiConnection.SendQueryAsync<NewReturning>(ModellingQueries.newAppServer, Variables) ).ReturnIds;
                if (returnIds != null)
                {
                    ActAppServer.Id = returnIds[0].NewId;
                    await LogChange(ModellingTypes.ChangeType.Insert, ModellingTypes.ModObjectType.AppServer, ActAppServer.Id,
                        $"New App Server: {ActAppServer.Display()}", Application.Id);
                    AvailableAppServers.Add(ActAppServer);
                }
                return true;
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Uniqueness violation"))
                {
                    DisplayMessageInUi(null, userConfig.GetText("E9010"), "", true);
                }
                else
                {
                    DisplayMessageInUi(exception, userConfig.GetText("add_app_server"), "", true);
                }
                return false;
            }
        }

        private async Task<bool> UpdateAppServerInDb()
        {
            try
            {
                var Variables = new
                {
                    id = ActAppServer.Id,
                    name = ActAppServer.Name,
                    appId = Application.Id,
                    ip = ActAppServer.Ip,
                    ipEnd = ActAppServer.IpEnd,
                    importSource = GlobalConst.kManual,
                    customType = ActAppServer.CustomType
                };
                await apiConnection.SendQueryAsync<ReturnId>(ModellingQueries.updateAppServer, Variables);
                await LogChange(ModellingTypes.ChangeType.Update, ModellingTypes.ModObjectType.AppServer, ActAppServer.Id,
                    $"Updated App Server: {ActAppServer.Display()}", Application.Id);
                return true;
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Uniqueness violation"))
                {
                    DisplayMessageInUi(null, userConfig.GetText("E9010"), "", true);
                }
                else
                {
                    DisplayMessageInUi(exception, userConfig.GetText("edit_app_server"), "", true);
                }
                return false;
            }
        }
    }
}
