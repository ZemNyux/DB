#define WIN32_LEAN_AND_MEAN

#include <iostream>
#include <string>
#include <windows.h>
#include <ws2tcpip.h>
using namespace std;

#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")

#define DEFAULT_BUFLEN 512
#define DEFAULT_PORT "27015"

int main() {
    SetConsoleOutputCP(65001);
    SetConsoleCP(65001);

    system("title CLIENT");
    cout << "Client started!\n";

    WSADATA wsaData;
    int iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (iResult != 0) {
        cout << "WSAStartup failed: " << iResult << "\n";
        return 1;
    }

    addrinfo hints{};
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    addrinfo* result = NULL;
    iResult = getaddrinfo("localhost", DEFAULT_PORT, &hints, &result);
    if (iResult != 0) {
        cout << "getaddrinfo failed: " << iResult << "\n";
        WSACleanup();
        return 2;
    }

    SOCKET ConnectSocket = INVALID_SOCKET;
    for (addrinfo* ptr = result; ptr != NULL; ptr = ptr->ai_next) {
        ConnectSocket = socket(ptr->ai_family, ptr->ai_socktype, ptr->ai_protocol);
        if (ConnectSocket == INVALID_SOCKET) {
            cout << "socket failed: " << WSAGetLastError() << "\n";
            WSACleanup();
            return 3;
        }

        iResult = connect(ConnectSocket, ptr->ai_addr, (int)ptr->ai_addrlen);
        if (iResult == SOCKET_ERROR) {
            closesocket(ConnectSocket);
            ConnectSocket = INVALID_SOCKET;
            continue;
        }
        break;
    }
    freeaddrinfo(result);

    if (ConnectSocket == INVALID_SOCKET) {
        cout << "Could not connect to server!\n";
        WSACleanup();
        return 4;
    }

    cout << "Connected to server!\n";

    while (true) {
        cout << "Enter a number (or Ctrl+C to exit): ";
        int number;
        if (!(cin >> number)) break;

        string message = to_string(number);
        iResult = send(ConnectSocket, message.c_str(), message.size(), 0);
        if (iResult == SOCKET_ERROR) {
            cout << "send failed: " << WSAGetLastError() << "\n";
            break;
        }

        char answer[DEFAULT_BUFLEN]{};
        iResult = recv(ConnectSocket, answer, DEFAULT_BUFLEN - 1, 0);
        if (iResult > 0) {
            answer[iResult] = '\0';
            cout << "Server responded: " << answer << "\n\n";
        }
        else if (iResult == 0)
            cout << "Connection closed by server.\n";
        else {
            cout << "recv failed: " << WSAGetLastError() << "\n";
            break;
        }
    }

    closesocket(ConnectSocket);
    WSACleanup();
    cout << "Client finished.\n";
}