using FWO.Api.Data;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using RestSharp.Serializers;
using Microsoft.IdentityModel.Tokens;
using FWO.Logging;
using System.Text.Json.Serialization; 
using Newtonsoft.Json;


namespace FWO.Tufin.SecureChange
{
	public class SCTicket : ExternalTicket
	{
        [JsonProperty("subject"), JsonPropertyName("subject")]
		public string Subject { get; set; } = "";

		private string OnBehalfUser { get; set; } = "";

		// public SCTicket(List<ModellingConnection> connections, string subject, ExternalTicketPriority priority = ExternalTicketPriority.Normal)
		// {
		// 	foreach (ModellingConnection conn in connections)
		// 	{
		// 		TicketTasks.Add(new ExternalAccessRequestTicketTask(conn));
		// 	}
		// 	Subject = subject;
		// 	Priority = (int) priority;
		// }

		public SCTicket(List<WfReqTask> tasks, string subject, ExternalTicketPriority priority = ExternalTicketPriority.Normal)
		{
			foreach (var task in tasks)
			{
				TicketTasks.Add(new ExternalAccessRequestTicketTask(task));
			}
			Subject = subject;
			// Priority = (int) priority;
		}

		// public void AddTask(ModellingConnection connection)
		// {
		// 	TicketTasks.Add(new ExternalAccessRequestTicketTask(connection));
		// }

		private void ConfigureRestClientSerialization(SerializerConfig config)
		{
			JsonNetSerializer serializer = new (); // Case insensivitive is enabled by default
			config.UseSerializer(() => serializer);
		}

		public async Task<RestResponse<int>> CreateTicketInTufin(ExternalTicketSystem tufinSystem, string taskType)
		{
			string ticketText = "";
			string taskText = "";

			ExternalTicketTemplate? template = tufinSystem.Templates.FirstOrDefault(t => t.TaskType == taskType);			
			if (template != null)
			{
				TicketTemplate = template.TicketTemplate;
				TasksTemplate = template.TasksTemplate;
			}

			// create text for all tasks/connections
			taskText = string.Join(",", TicketTasks.ConvertAll(t => t.FillTaskTemplate(TasksTemplate)));
			// foreach (ExternalAccessRequestTicketTask ticketTask in TicketTasks)
			// {
			// 	taskText += ticketTask.FillTaskTemplate(TasksTemplate) + ",";
			// }
			// taskText = taskText.TrimEnd(',');

			// substitute ticket template data
			ticketText = TicketTemplate
				.Replace("@@TICKET_SUBJECT@@", "test ticket create connection1")
				.Replace("@@PRIORITY@@", "Normal")
				// .Replace("@@ONBEHALF@@", OnBehalfOfUser)
				// .Replace("@@WORKFLOW_NAME@@", "workflow_name")
				.Replace("@@TASKS@@", taskText);

			// build API call
			RestRequest request = new("tickets.json", Method.Post);
			request.AddJsonBody(ticketText);
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Authorization", tufinSystem.Authorization);
			RestClientOptions restClientOptions = new();
			restClientOptions.RemoteCertificateValidationCallback += (_, _, _, _) => true;
			restClientOptions.BaseUrl = new Uri(tufinSystem.Url);
			RestClient restClient = new(restClientOptions, null, ConfigureRestClientSerialization);

			// Debugging SecureChange API call
			string headers = "";
			string body = "";
			foreach (Parameter p in request.Parameters)
			{
				if (p.Name == "")
				{
					body = $"--data '{p.Value}'";
				}
				else
				{
					if (p.Name != "Authorization") // avoid logging of credentials
						headers += $"--header '{p.Name}: {p.Value}' ";
				}
			}
			Log.WriteDebug("API", $"Sending API Call to SecureChange:\ncurl --insecure --request {request.Method} --url {restClient.Options.BaseUrl} {body} {headers} ");

			// send API call
			return await restClient.ExecuteAsync<int>(request);
		}
	}
}

	/*

		Create Ticket Sample Call

		curl --request POST \
			--insecure \
			--url https://tufin-stest.xxx.de/securechangeworkflow/api/securechange/tickets.json \
			--header 'Authorization: Basic xxx' \
			--header 'Content-Type: application/json' \
			--data '{
				"ticket": {
					"subject": "NeMo-Testing",
					"priority": "Normal",
					"domain_name": "",
					"workflow": {
						"name": "1. xxx Standard Firewall Request"
					},
					"steps": {
						"step": [
							{
								"name": "Erfassung des Antrags",
								"tasks": {
									"task": {
										"fields": {
											"field": [
												{
													"@xsi.type": "multi_access_request",
													"name": "Gewünschter Zugang",
													"read_only": false,
													"access_request": {
														"order": "AR1",
														"verifier_result": {
															"status": "not run"
														},
														"use_topology": true,
														"targets": {
															"target": {
																"@type": "ANY"
															}
														},
														"users": {
															"user": [
																"Any"
															]
														},
														"sources": {
															"source": [
																{
																	"@type": "IP",
																	"ip_address": "10.10.100.10",
																	"netmask": "255.255.255.255",
																	"cidr": 32
																}
															]
														},
														"destinations": {
															"destination": [
																{
																	"@type": "IP",
																	"ip_address": "10.20.200.0",
																	"netmask": "255.255.255.128",
																	"cidr": 25
																}
															]
														},
														"services": {
															"service": [
																{
																	"@type": "PREDEFINED",
																	"protocol": "TCP",
																	"port": 23,
																	"predefined_name": "telnet"
																},
																{
																	"@type": "PREDEFINED",
																	"protocol": "TCP",
																	"port": 79,
																	"predefined_name": "finger"
																},
																{
																	"@type": "PROTOCOL",
																	"protocol": "TCP",
																	"port": 90
																}
															]
														},
														"action": "Accept",
														"labels": ""
													}
												},
												{
													"@xsi.type": "text_area",
													"name": "Grund für den Antrag",
													"read_only": false,
													"text": "dsadsa"
												},
												{
													"@xsi.type": "drop_down_list",
													"name": "Regel Log aktivieren?",
													"selection": "Ja"
												},
												{
													"@xsi.type": "date",
													"name": "Regel befristen bis:"
												},
												{
													"@xsi.type": "text_field",
													"name": "Anwendungs-ID",
													"text": "APP-3838"
												},
												{
													"@xsi.type": "checkbox",
													"name": "Die benötigte Kommunikationsverbindung ist im Kommunikationsprofil nach IT-Sicherheitsstandard hinterlegt",
													"value": false
												},
												{
													"@xsi.type": "drop_down_list",
													"name": "Expertenmodus: Exakt wie beantragt implementieren (Designervorschlag ignorieren)",
													"selection": "Nein"
												}
											]
										}
									}
								}
							}
						]
					}
				}
			}'

	*/

