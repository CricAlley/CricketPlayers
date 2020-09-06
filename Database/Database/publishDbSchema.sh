#!/bin/bash
ls
sqlpackage /a:Publish /tcs:"$1" /sf:Database.dacpac
