#include <iostream>
#include <algorithm>
#include <string>
#include <ws2tcpip.h> // тип WSADATA, функції WSAStartup, WSACleanup та багато іншого
#include <windows.h>
using namespace std;

#pragma comment (lib, "Ws2_32.lib")

#define DEFAULT_BUFLEN 512
#define DEFAULT_PORT "27015" // порт — це логічна конструкція, яка ідентифікує конкретний процес або тип мережевої служби - https://en.wikipedia.org/wiki/Port_(computer_networking)

#define PAUSE 1

int main() {
    SetConsoleCP(65001);
    SetConsoleOutputCP(65001);
    system("title СЕРВЕРНА СТОРОНА");
    cout << "серверний процес запущено!\n";
    Sleep(PAUSE);

    WSADATA wsaData; // структура WSADATA містить інформацію про реалізацію Windows Sockets: https://docs.microsoft.com/en-us/windows/win32/api/winsock/ns-winsock-wsadata
    int iResult = WSAStartup(MAKEWORD(2, 2), &wsaData); // ініціалізація бібліотеки Winsock DLL: https://firststeps.ru/mfc/net/socket/r.php?2

    addrinfo hints{}; // структура addrinfo використовується для зберігання інформації про адресу хоста
    hints.ai_family = AF_INET; // сімейство адрес для IPv4
    hints.ai_socktype = SOCK_STREAM; // протокол TCP
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE; // використовується для прив'язки сокета

    addrinfo* result = NULL;
    iResult = getaddrinfo(NULL, DEFAULT_PORT, &hints, &result); // отримання адреси та порту сервера

    SOCKET ListenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol); // створення сокета

    iResult = bind(ListenSocket, result->ai_addr, (int)result->ai_addrlen); // прив'язка сокета

    freeaddrinfo(result);

    iResult = listen(ListenSocket, SOMAXCONN); // переведення сокета в стан очікування вхідних з'єднань

    SOCKET ClientSocket = accept(ListenSocket, NULL, NULL); // прийняття вхідного з'єднання

    closesocket(ListenSocket); // закриття сокета прослуховування, оскільки з'єднання встановлено

    char recvbuf[DEFAULT_BUFLEN]{}; // буфер для отримання даних
    int iSendResult;

    do {
        iResult = recv(ClientSocket, recvbuf, DEFAULT_BUFLEN, 0); // отримання даних від клієнта
        if (iResult > 0) {
            cout << "байти отримано: " << iResult << "\n";
            cout << "повідомлення: " << recvbuf << "\n";

            // перевертаємо рядок
            string reversed(recvbuf, iResult);
            reverse(reversed.begin(), reversed.end());

            iSendResult = send(ClientSocket, reversed.c_str(), reversed.size(), 0);
            if (iSendResult == -1) {
                cout << "надсилання не вдалося з помилкою: " << WSAGetLastError() << "\n";
                closesocket(ClientSocket);
                WSACleanup();
                return 7;
            }
            cout << "надіслано байт: " << iSendResult << "\n";
        }
        else if (iResult == 0)
            cout << "з'єднання закрито\n";
        else {
            cout << "recv не вдалося з помилкою: " << WSAGetLastError() << "\n";
            closesocket(ClientSocket);
            WSACleanup();
            return 8;
        }

    } while (iResult > 0);

    closesocket(ClientSocket); // закриття клієнтського сокета
    WSACleanup(); // завершення роботи з бібліотекою Winsock
}