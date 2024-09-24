﻿using FWO.Api.Client;
using FWO.Api.Client.Queries;
using FWO.Api.Data;
using FWO.Config.Api;
using FWO.Logging;
using System.Net;
using RestSharp;
using FWO.Tufin.SecureChange;


namespace FWO.Middleware.Server
{
	/// <summary>
	/// Class handling the sending of external requests
	/// </summary>
	public class ExternalRequestSender
	{
		/// <summary>
		/// Api Connection
		/// </summary>
		protected readonly ApiConnection apiConnection;

		/// <summary>
		/// Global Config
		/// </summary>
		protected GlobalConfig globalConfig;


		private bool WorkInProgress = false;
		private readonly UserConfig userConfig;
		private List<ExternalRequest> openRequests = [];


		/// <summary>
		/// Constructor for External Request Sender
		/// </summary>
		public ExternalRequestSender(ApiConnection apiConnection, GlobalConfig globalConfig)
		{
			this.apiConnection = apiConnection;
			this.globalConfig = globalConfig;
			userConfig = new(globalConfig);
		}

		/// <summary>
		/// Run the External Request Sender
		/// </summary>
		public async Task<bool> Run()
		{
			try
			{
				if(!WorkInProgress)
				{
					WorkInProgress = true;
					openRequests = await apiConnection.SendQueryAsync<List<ExternalRequest>>(ExtRequestQueries.getOpenRequests);
					foreach(var request in openRequests)
					{
						if(request.ExtRequestState == ExtStates.ExtReqInitialized.ToString())
						{
							await SendRequest(request);
						}
						else
						{
							await RefreshState(request);
						}
					}
					WorkInProgress = false;
				}
			}
			catch(Exception exception)
			{
				Log.WriteError("External Request Sender", $"Runs into exception: ", exception);
				WorkInProgress = false;
				return false;
			}
			return true;
		}

		private async Task SendRequest(ExternalRequest request)
		{
			try
			{
				RestResponse<int>? ticketIdResponse = null;
            	ExternalTicketSystem extTicketSystem = System.Text.Json.JsonSerializer.Deserialize<ExternalTicketSystem>(request.ExtTicketSystem) ?? throw new Exception("No Ticket System");
				if(extTicketSystem.Type == ExternalTicketSystemType.TufinSecureChange)
				{
					SCTicket ticket = System.Text.Json.JsonSerializer.Deserialize<SCTicket>(request.ExtRequestContent) ?? throw new Exception("No Ticket Content");
                	ticketIdResponse = await ticket.CreateTicketInTufin(extTicketSystem, request.ExtRequestType);
				}
				if (ticketIdResponse != null)
				{
					if (ticketIdResponse.StatusCode == HttpStatusCode.OK)
					{
						await UpdateState(request, ExtStates.ExtReqRequested.ToString());
					}
					else
					{
						Log.WriteError(userConfig.GetText("ext_ticket_fail"), "Error Message: " + ticketIdResponse?.StatusDescription + ", " + ticketIdResponse?.ErrorMessage);
					}
				}
				else
				{
					Log.WriteError(userConfig.GetText("ext_ticket_fail"), "No ticket sent.");
				}
			}
			catch(Exception exception)
			{
				Log.WriteError("External Request Sender", $"Sending request failed: ", exception);
			}
		}

		private async Task RefreshState(ExternalRequest request)
		{
			try
			{
				string newState = await CheckState(request);
				if(newState != request.ExtRequestState)
				{
					await UpdateState(request, newState);
				}
			}
			catch(Exception exception)
			{
				Log.WriteError("External Request Sender", $"Status update failed: ", exception);
			}
		}

		private async Task<string> CheckState(ExternalRequest request)
		{
			// todo
			return request.ExtRequestState;
		}

		private async Task UpdateState(ExternalRequest request, string newState)
		{
			try
			{
				var Variables = new
				{
					id = request.Id,
					extRequestState = newState
				};
				await apiConnection.SendQueryAsync<ReturnId>(ExtRequestQueries.updateExtRequestState, Variables);
			}
			catch(Exception exception)
			{
				Log.WriteError("External Request Sender", $"State update failed: ", exception);
			}
		}
	}
}
