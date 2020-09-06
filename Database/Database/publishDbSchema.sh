#!/bin/bash
sqlpackage /a:Publish /tcs:"$1" /sf:"Database.dacpac"
