#!/bin/bash
sqlpackage /a:Publish /tcs:"$1" /sf:"$(2)/Database.dacpac"
