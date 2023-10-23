kubectl create namespace finance-manager

kubectl config set-context --current --namespace finance-manager

helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx/
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

helm install nginx --namespace finance-manager ingress-nginx/ingress-nginx -f nginx-ingress-values.yml --atomic

helm install postgres --namespace finance-manager bitnami/postgresql -f postgres-values.yml --atomic

helm install rabbit --namespace finance-manager bitnami/rabbitmq  -f rabbit-values.yml --atomic

kubectl create secret generic secret-user-appsettings --namespace finance-manager --from-file=./FinanceManager.User/appsettings.secrets.json
kubectl create secret generic secret-account-appsettings --namespace finance-manager --from-file=./FinanceManager.Account/appsettings.secrets.json
kubectl create secret generic secret-deposit-appsettings --namespace finance-manager --from-file=./FinanceManager.Deposit/appsettings.secrets.json
kubectl create secret generic secret-expense-appsettings --namespace finance-manager --from-file=./FinanceManager.Expense/appsettings.secrets.json
kubectl create secret generic secret-transfer-appsettings --namespace finance-manager --from-file=./FinanceManager.Transfer/appsettings.secrets.json
kubectl create secret generic secret-file-appsettings --namespace finance-manager --from-file=./FinanceManager.File/appsettings.secrets.json
kubectl create secret generic secret-notification-appsettings --namespace finance-manager --from-file=./FinanceManager.Notification/appsettings.secrets.json
kubectl create secret generic secret-statistics-appsettings --namespace finance-manager --from-file=./FinanceManager.Statistics/appsettings.secrets.json

kubectl apply -f manifest.yml
