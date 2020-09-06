#!/bin/bash
echo "$(ConnectionStrings.CricketDB)"
echo  $(ConnectionStrings.CricketDB)
echo  $ConnectionStrings.CricketDB
echo  "$ConnectionStrings.CricketDB"
echo "$env:CONNECTIONSTRINGS_CRICKETDB"
echo "$CONNECTIONSTRINGS_CRICKETDB"
echo  $CONNECTIONSTRINGS_CRICKETDB
echo  $(CONNECTIONSTRINGS_CRICKETDB)
echo  "$(CONNECTIONSTRINGS_CRICKETDB)"

sqlpackage /a:Publish /tcs="$(ConnectionStrings.CricketDB)" /sf=Database.dacpac