module Contextive.Cloud.Api.SlackModels

module Receiving =
    type SlackMessage = {
        Type: string
        Channel: string
        Text: string
        User: string
        Channel_Type: string
        Ts: string
        Event_ts: string
    }

    type SlackEvent = {
        Token: string
        Challenge: string 
        Type: string
        Event: SlackMessage
    }

module Sending =
    open FSharp.Data
    type SlackMessage = JsonProvider<"""{
    "channel": "channel",
	"thread_ts": "thread_ts",
	"blocks": [
		{
			"type": "header",
			"text": {
				"type": "plain_text",
				"text": "Providers Context"
			}
		},
		{
			"type": "context",
			"elements": [
				{
					"type": "mrkdwn",
					"text": "*Provider*\nA Provider is an person that does the work"
				}
			]
		},
		{
			"type": "actions",
			"elements": [
				{
					"type": "button",
					"text": {
						"type": "plain_text",
						"text": "Update Definition"
					},
					"value": "update"
				},
				{
					"type": "button",
					"text": {
						"type": "plain_text",
						"text": "Usage Examples"
					},
					"value": "usage"
				},
				{
					"type": "button",
					"text": {
						"type": "plain_text",
						"text": "Usage in Other Contexts"
					},
					"value": "usage"
				}
			]
		},
		{
			"type": "divider"
		},
		{
			"type": "context",
			"elements": [
				{
					"type": "mrkdwn",
					"text": "*Biller*\nA Biller is an org that invoices for the work"
				}
			]
		},
		{
			"type": "actions",
			"elements": [
				{
					"type": "button",
					"text": {
						"type": "plain_text",
						"text": "Update Definition"
					},
					"value": "update"
				},
				{
					"type": "button",
					"text": {
						"type": "plain_text",
						"text": "Usage Examples"
					},
					"value": "usage"
				},
				{
					"type": "button",
					"text": {
						"type": "plain_text",
						"text": "Usage in Other Contexts"
					},
					"value": "usage"
				}
			]
		},
		{
			"type": "divider"
		},
		{
			"type": "context",
			"elements": [
				{
					"type": "mrkdwn",
					"text": "These definitions will not be shown for another two weeks."
				}
			]
		},
		{
			"type": "actions",
			"elements": [
				{
					"type": "button",
					"text": {
						"type": "plain_text",
						"text": "Change Reminder Frequence"
					},
					"value": "change_reminder_frequence"
				}
			]
		}
	]
}""">
