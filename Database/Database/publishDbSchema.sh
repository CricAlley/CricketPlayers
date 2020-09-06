#!/bin/bash
sqlpackage /a:Publish /tcs:"$CONNECTIONSTRINGS_CRICKETDB" /sf:Database.dacpac
sqlpackage /a:Publish /tcs:$CONNECTIONSTRINGS_CRICKETDB /sf:Database.dacpac
sqlpackage /a:Publish /tcs:"$(CONNECTIONSTRINGS_CRICKETDB)" /sf:Database.dacpac
sqlpackage /a:Publish /tcs:$(CONNECTIONSTRINGS_CRICKETDB) /sf:Database.dacpac