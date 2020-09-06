#!/bin/bash
echo $(pwd)
sqlpackage /a:Publish /tcs:"$1" /sf:Database.dacpac
