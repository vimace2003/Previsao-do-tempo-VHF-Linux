# Documentação do Sistema de Previsão do Tempo e Transmissão via Rádio

## Descrição

Este programa em C# consulta a previsão do tempo para cidades específicas utilizando a API da OpenWeatherMap, gera um áudio com as informações obtidas usando a API de fala do Microsoft Azure e transmite o áudio via rádio usando um sinal RTS pela porta serial.

## Funcionalidades

1. **Consulta da Previsão do Tempo**:
   - Obtém a previsão do tempo (incluindo temperatura, umidade, pressão, velocidade e direção do vento, entre outros) para uma cidade escolhida aleatoriamente de uma lista (`cities.txt`).
   - Utiliza a API da OpenWeatherMap para coletar dados em tempo real.
   
2. **Geração de Áudio**:
   - Utiliza a API de fala do Microsoft Azure para gerar o áudio com as informações do tempo.
   - Suporte a múltiplas vozes brasileiras, escolhidas aleatoriamente a cada execução.

3. **Transmissão via Porta Serial**:
   - O áudio gerado é transmitido via rádio, com controle do RTS da porta serial para ativar e desativar o rádio.

4. **Mensagens Personalizadas**:
   - Permite a inclusão de uma mensagem personalizada (`custom_message.txt`) no áudio gerado, se o arquivo estiver presente.

## Configuração

### Arquivo `appsettings.json`

O programa carrega algumas configurações através do arquivo `appsettings.json`. Exemplo de configuração:

```json
{
  "ApiKey": "SUA_API_KEY_OPENWEATHERMAP",
  "SerialPortName": "COM3",
  "CallSign": "PP5KJ"
}
```
Arquivo cities.txt

O arquivo cities.txt contém a lista de cidades que serão consultadas. O formato esperado para cada linha é:

mathematica

Cidade,Latitude,Longitude

**Exemplo:**

```css
Florianópolis,-27.5954,-48.5480
Palhoça,-27.6455,-48.6693
São José,-27.6136,-48.6366
```
Arquivo custom_message.txt (Opcional)

Este arquivo permite adicionar uma mensagem personalizada ao final de cada transmissão. Se o arquivo não existir, o programa continua normalmente sem a mensagem adicional.
Dependências

    Bibliotecas Externas:
        Newtonsoft.Json para manipulação de JSON.
        Microsoft.Extensions.Configuration para carregar as configurações do appsettings.json.

    APIs Utilizadas:
        OpenWeatherMap: Para obter as condições climáticas e previsões.
        Microsoft Azure Speech: Para converter o texto em áudio.

**Como Funciona**

Inicialização:
    O programa carrega as configurações do arquivo appsettings.json.
    Lê a lista de cidades do arquivo cities.txt.
    Lê a mensagem personalizada (se disponível) do arquivo custom_message.txt.

Consulta da Previsão do Tempo:
Seleciona uma cidade aleatoriamente e consulta os dados de tempo atual e previsão futura usando as APIs da OpenWeatherMap.
Geração do Áudio:

Monta uma mensagem de áudio contendo as informações meteorológicas e a mensagem personalizada.
Utiliza a API da Microsoft Azure para converter o texto em áudio.

Transmissão via Rádio:
Abre a porta serial configurada e ativa o sinal RTS para iniciar a transmissão.
Reproduz o áudio gerado através do comando aplay (Linux).
Desativa o sinal RTS após o fim da reprodução.

**Como Executar**

Pré-requisitos:
    Instalar o .NET SDK.
    Configurar as bibliotecas necessárias (Newtonsoft.Json, Microsoft.Extensions.Configuration).
    Ter uma conta no OpenWeatherMap e Microsoft Azure com as respectivas chaves de API.

Executando o Programa:
    Crie o arquivo appsettings.json com suas configurações.
    Crie o arquivo cities.txt com as cidades de interesse.
    Execute o programa no terminal/console:

    dotnet run

**Erros Comuns**

Erro de Porta Serial: Verifique se a porta serial configurada em appsettings.json está correta e se o dispositivo está conectado.
Erro de API Key: Certifique-se de que as chaves de API para OpenWeatherMap e Microsoft Azure estão corretas e ativas.

**Exemplo de Uso**

Ao executar o programa, o sistema gerará uma saída similar a esta:

```yaml

Configurações carregadas:
API Key: SUA_API_KEY_OPENWEATHERMAP
Serial Port Name: COM3
Call Sign: PP5KJ
Porta serial COM3 aberta.
Consultando o tempo para Florianópolis...
Latitude: -27.5954, Longitude: -48.5480
Dados do tempo recebidos com sucesso.
Gerando áudio com Azure Speech...
Áudio gerado.
Sinal RTS ativado.
Áudio reproduzido.
Sinal RTS desativado.
Porta serial COM3 fechada.
```
Referências

API OpenWeatherMap

API Microsoft Azure Speech
