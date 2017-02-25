# Pioneer Warmer
Pioneer Warmer is a Windows Service used to serve the following needs.

* Keep your site from going idle.
  * On budget hosting plans...
    * Often a site that has not received traffic from a pre-determined amount of time will be paused or stopped to free up resources on the server in question.
    * This causes the next incoming request to that paused/stopped site to trigger a restart.
      * Causing assets to be recompiled.  
      * Of which negatively affects the performance (load time) of that request. 
* Determine if x-number of pages are valid.
  * A valid state means the service is able to identify a user-defined token on any of the pages in a collection.
  * A non-valid state might include...
    * Your web hosting company often puts up their own critical error pages when they are having issues internally.
      * Being this page will not have your user-defined token, an invalid state will be triggered and an email will be sent.

## How it works
Pioneer Warmer makes a request to a defined collection of pages at a defined interval. 
It then measures the length of the request to determine it falls inside the bounds defined in config. 
If it does not fall inside these bound it sends an email notification. 
If it does fall inside these bounds it then parses the response looking for a defined token. 
If that token is not found, it sends an email notification. 

## Config
You can use the [config-example.sjon](config-example.json) as a starting point and refernce [Config.cs](config.cs) for more details.

```json
{
  "TimerResolution": 30,
  "WarmOneRandomPagePerTimerLoop": true,
  "EmailTo": "email_to_notify@email.com",
  "EmailFrom": "email_from@email.com",
  "EmailUsername": "username",
  "EmailPassword": "password",
  "EmailHost": "mail.host.com",
  "EmailHostPort": 587,
  "Pages": [
    {
      "Url": "http://site-to-warm.com",
      "Token": "some unique token",
      "ReponseThreshold": 3
    },
    {
      "Url": "http://site-to-warm.com",
      "Token": "some unique token",
      "ReponseThreshold": 3
    },
    {
      "Url": "http://site-to-warm.com",
      "Token": "some unique token",
      "ReponseThreshold": 3
    }
  ]
}
```


## Install
Pioneer Warmer requires .NET 4.6.1 to be installed on the machine it lives.

Download and compile the project. Grab the resources and place them on the machine of your choice. 

 Run the following
```bash
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe" "C:\pioneer\pioneer.warmer\Pioneer.Warmer.Service.exe"
```

## Uninstall

Run the following
```bash
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe" "C:\pioneer\pioneer.warmer\Pioneer.Warmer.Service.exe" /u
```
