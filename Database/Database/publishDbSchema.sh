#!/bin/bash
sqlpackage /a:Publish /tcs:$ConnectionStrings_CricketDB /sf:Database.dacpac
sqlpackage /a:Publish /tcs:"$(ConnectionStrings_CricketDB)" /sf:Database.dacpac
sqlpackage /a:Publish /tcs:$(ConnectionStrings_CricketDB) /sf:Database.dacpac
sqlpackage /a:Publish /tcs:"$ConnectionStrings_CricketDB" /sf:Database.dacpac