# O projeto Vendinha foi desenvolvido em C# com Windows Forms e PostgreSQL. O objetivo é criar um sistema simples para cadastrar clientes e controlar as dívidas de uma pequena vendinha.

# 

# O programa permite cadastrar, editar, excluir e pesquisar clientes pelo nome. Também calcula a idade automaticamente e verifica se o CPF possui exatamente 11 números, não permitindo cadastrar CPFs repetidos.

# 

# Na parte das dívidas, é possível cadastrar valores, consultar o histórico e registrar pagamentos. Cada cliente pode ter apenas uma dívida em aberto, e o sistema mostra o total que ele está devendo.

# 

# Os clientes são organizados pelo maior valor devido, com uma listagem de 10 em 10.

# 

# Para desenvolver o sistema, utilizamos C# com .NET 8, Windows Forms e PostgreSQL. O DBeaver foi usado para administrar o banco de dados e o Npgsql para fazer a conexão com o programa.

# 

# O banco se chama techcore e possui duas tabelas: clientes e dividas.

# 

# Para executar o projeto, é necessário abrir o arquivo Vendinha.csproj no Visual Studio, ter acesso ao PostgreSQL e criar as tabelas usando o arquivo criar\_banco.sql, que está na pasta BancoDeDados. Também é preciso configurar a variável VENDINHA\_CONNECTION com os dados de acesso ao banco. Depois disso, basta pressionar F5 para iniciar o programa.

# 

# O código foi organizado em classes e formulários para facilitar o desenvolvimento e a manutenção do sistema.

