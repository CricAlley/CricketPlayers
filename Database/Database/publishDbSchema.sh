#!/bin/bash
sqlpackage /a:Publish /tsn:$(DbServerName) /tdn:$(DbName) /tu:$(DbUser) /tp:$(DbPassword) /sf:Database.dacpac

