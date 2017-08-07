# ups_battery_service
Creates Windows event log entries on system battery events

# Information
This is a quite old project which I republished due to demand.
The service generates Windows event log entries (on local and remote machines) when the locally attached battery (e.g. UPS) is low or not available.
You can attach events like sending informative emails, shutdown or do other tasks then with the native methods of Windows (scheduled tasks).

# Installation
- Run setup.exe / setup.msi
- Setup asks for user credentials for the Windows service. Depending on what permissions it needs (e.g. for writing remote event logs), use a Windows user with appropriate permissions.
- Start the service.

# Optional
You might want to have a look at the ups_battery_service.config file for configuration options.
E.g. you can insert multiple remote servers. This can be useful if multiple systems are attached to the same UPS, so every machine gets an event log entry.

# Link
http://www.dxsdata.com/blog