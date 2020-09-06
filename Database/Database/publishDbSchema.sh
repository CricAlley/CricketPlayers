#!/bin/bash
sudo ls
sqlpackage /a:Publish /tcs:"$1" /sf:Database.dacpac
