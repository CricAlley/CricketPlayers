#!/bin/bash
sqlpackage /a:Publish /tcs=$(ConnectionStrings.CricketDB) /sf=Database.dacpac