#!/bin/bash
today=`date +%d%b%y_%H%M`
SQLFILE=~/backup/backup
DATABASE=plan_suite
USER=plan-suite

echo "-> Building production"
if dotnet publish ~/build/plan-suite/PlanSuite/PlanSuite.csproj --configuration Release --runtime linux-x64 --self-contained --output ~/build/Output
then
    cp ~/build/appsettings.json Output/appsettings.json
    cp -R ~/build/Output/* /var/www/plansuite
    mysqldump --defaults-file=~/.my.cnf -u ${USER} ${DATABASE}|gzip > ${SQLFILE}-$today.sql.gz
    sudo service plansuite restart
else
    echo "-> Failed to build production build due to .NET errors."
    exit 1
fi