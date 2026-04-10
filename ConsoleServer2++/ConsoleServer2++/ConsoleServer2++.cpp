#include <iostream>
#include <algorithm>
#include <string>
#include <ws2tcpip.h>
#include <windows.h>
using namespace std;

#pragma comment (lib, "Ws2_32.lib")

#define DEFAULT_BUFLEN 512
#define DEFAULT_PORT "27015"

int main() {
    SetConsoleOutputCP(65001);
    SetConsoleCP(65001);

    system("title SERVER");
    cout << "Server started!\n";

    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);

    addrinfo hints{};
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    addrinfo* result = NULL;
    getaddrinfo(NULL, DEFAULT_PORT, &hints, &result);

    SOCKET ListenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
    bind(ListenSocket, result->ai_addr, (int)result->ai_addrlen);
    freeaddrinfo(result);
    listen(ListenSocket, SOMAXCONN);

    cout << "Waiting for client...\n";
    SOCKET ClientSocket = accept(ListenSocket, NULL, NULL);
    closesocket(ListenSocket);
    cout << "Client connected!\n";

    char recvbuf[DEFAULT_BUFLEN]{};
    int iResult;

    do {
        iResult = recv(ClientSocket, recvbuf, DEFAULT_BUFLEN, 0);
        if (iResult > 0) {
            recvbuf[iResult] = '\0';
            int number = atoi(recvbuf);
            cout << "Received from client: " << number << "\n";

            // число на 1 больше
            int response = number + 1;
            string responseStr = to_string(response);

            cout << "Sending back: " << response << "\n";
            send(ClientSocket, responseStr.c_str(), responseStr.size(), 0);
        }
        else if (iResult == 0)
            cout << "Connection closed.\n";
        else
            cout << "recv failed: " << WSAGetLastError() << "\n";

    } while (iResult > 0);

    closesocket(ClientSocket);
    WSACleanup();
}