docker image rm widedreadnout/finance-manager-user:latest
docker image build --no-cache -t widedreadnout/finance-manager-user:latest -f ./userDockerfile .
docker push widedreadnout/finance-manager-user:latest

docker image rm widedreadnout/finance-manager-account:latest
docker image build --no-cache -t widedreadnout/finance-manager-account:latest -f ./accountDockerfile .
docker push widedreadnout/finance-manager-account:latest

docker image rm widedreadnout/finance-manager-deposit:latest
docker image build --no-cache -t widedreadnout/finance-manager-deposit:latest -f ./depositDockerfile .
docker push widedreadnout/finance-manager-deposit:latest

docker image rm widedreadnout/finance-manager-expense:latest
docker image build --no-cache -t widedreadnout/finance-manager-expense:latest -f ./expenseDockerfile .
docker push widedreadnout/finance-manager-expense:latest

docker image rm widedreadnout/finance-manager-file:latest
docker image build --no-cache -t widedreadnout/finance-manager-file:latest -f ./fileDockerfile .
docker push widedreadnout/finance-manager-file:latest

docker image rm widedreadnout/finance-manager-notification:latest
docker image build --no-cache -t widedreadnout/finance-manager-file:latest -f ./fileDockerfile .
docker push widedreadnout/finance-manager-file:latest

docker image rm widedreadnout/finance-manager-statistics:latest
docker image build --no-cache -t widedreadnout/finance-manager-statistics:latest -f ./statisticsDockerfile .
docker push widedreadnout/finance-manager-statistics:latest
