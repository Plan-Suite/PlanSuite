#!/bin/bash
# We're keeping a sudo cache now so we can run root cmds without needing to input password later.

today=`date +%d%b%y_%H%M`
USER=`whoami`
SQLFILE=~/backup/backup
DATABASE=plan_suite
USER=plan-suite
OUTPUT=~/build/Output
GIT=git@github.com:Plan-Suite/PlanSuite.git
BRANCH=staging

echo "-> Downloading latest build"
cd ~/build
rm -rf ~/build/PlanSuite
git clone -b ${BRANCH} ${GIT}
cd ..

echo "-> Cleaning old build"
rm -rf ${OUTPUT}
mkdir -p ${OUTPUT}

echo "-> Building production"
cd ~/build/PlanSuite/PlanSuite
npm i
cd

if dotnet publish ~/build/PlanSuite/PlanSuite/PlanSuite.csproj --configuration Release --runtime linux-x64 --self-contained --output ${OUTPUT}
then
    echo "-> Copying files"
    cp ~/build/appsettings.json ~/build/Output/appsettings.json
    cp -R ${OUTPUT}/* /var/www/plansuite

    echo "-> Backing up database"
    mysqldump --defaults-file=~/.my.cnf -u ${USER} ${DATABASE}|gzip > ${SQLFILE}-$today.sql.gz

    echo "-> Applying new database changes"
    cd ~/build/PlanSuite/PlanSuite
    cp ~/build/appsettings.json ~/build/PlanSuite/PlanSuite/appsettings.json
    dotnet ef migrations bundle --self-contained -r linux-x64 --configuration Bundle
    ~/build/PlanSuite/PlanSuite/efbundle

    echo "-> Restarting website"
    sudo service plansuite restart

    echo "-> Done"
else
    echo "-> Failed to build production build due to .NET errors."
    exit 1
fi
