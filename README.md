# neo-modules-fura

## Introduction：
The data is directly recorded into the mongodb from the node through this plugin.

## How to use it:
- Clone the repository
- Publish this project
- Copy the .dll file generated by the this project publishing and the folder containing the configuration file to the Neo Plugins folder
- Copy all the .dll files under the lib folder to the Neo Plugins folder
- Modify Plugins/Fura/Config.json
- Start Up Neo Node

## Introduction of the configuration file:
```json
{
  "PluginConfiguration": {
    "DbName": "Neo",
    "Host": "127.0.0.1",
    "Port": 27017,
    "User": "",
    "Password": "",
    "Log": false,
    "ConnectionString": ""
  }
}
```
> 
> DbName : The name of the MongoDb repository, which must be filled in.
> 
> Host : The host of MongoDb, which must be filled in.
> 
> Port : The port of MongoDb, which must be filled in.
> 
> User : The userName of MongoDb, which must be filled in.
> 
> Password : The password of MongoDb, which must be filled in.
> 
> Log : Whether to start log output.
> 
> ConnectionString : The link url of MongoDb. Can be Empty. // such as mongodb://[username:password@]host1[:port1][,host2[:port2],...[,hostN[:portN]]][/[database][?options]]
