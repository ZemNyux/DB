#define WIN32_LEAN_AND_MEAN

#include <iostream>
#include <algorithm>
#include <string>
#include <windows.h>
#include <ws2tcpip.h>
using namespace std;

#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib") // AcceptEx(), ConnectEx(), WSASendMsg() тощо
#pragma comment (lib, "AdvApi32.lib") // Security API, Registry API, Service Control Manager тощо

#define DEFAULT_BUFLEN 512
#define DEFAULT_PORT "27015"

#define PAUSE 1

int main()
{
	SetConsoleCP(65001);
	SetConsoleOutputCP(65001);
	system("title КЛІЄНТСЬКА СТОРОНА");
	cout << "процес клієнта запущено!\n";
	Sleep(PAUSE);

	WSADATA wsaData;
	int iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (iResult != 0) {
		cout << "WSAStartup не вдалося з помилкою: " << iResult << "\n";
		return 11;
	}
	else {
		cout << "підключення до Winsock.dll пройшло успішно!\n";
		Sleep(PAUSE);
	}

	addrinfo hints{};
	hints.ai_family = AF_UNSPEC;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	const char* ip = "localhost"; // за замовчуванням, обидва додатки, і клієнт, і сервер, працюють на одній і тій же машині

	addrinfo* result = NULL;
	iResult = getaddrinfo(ip, DEFAULT_PORT, &hints, &result);

	if (iResult != 0) {
		cout << "getaddrinfo не вдалося з помилкою: " << iResult << "\n";
		WSACleanup();
		return 12;
	}
	else {
		cout << "отримання адреси та порту клієнта пройшло успішно!\n";
		Sleep(PAUSE);
	}

	SOCKET ConnectSocket = INVALID_SOCKET;

	for (addrinfo* ptr = result; ptr != NULL; ptr = ptr->ai_next) { // серверів може бути кілька, тому потрібен цикл
		ConnectSocket = socket(ptr->ai_family, ptr->ai_socktype, ptr->ai_protocol);

		if (ConnectSocket == INVALID_SOCKET) {
			cout << "сокет не вдалося створити з помилкою: " << WSAGetLastError() << "\n";
			WSACleanup();
			return 13;
		}

		iResult = connect(ConnectSocket, ptr->ai_addr, (int)ptr->ai_addrlen);
		if (iResult == SOCKET_ERROR) {
			closesocket(ConnectSocket);
			ConnectSocket = INVALID_SOCKET;
			continue;
		}

		cout << "створення сокета на клієнті пройшло успішно!\n";
		Sleep(PAUSE);

		break;
	}

	freeaddrinfo(result);


	while (true) {
		string message;
		cout << "введіть повідомлення: ";
		getline(cin, message);

		iResult = send(ConnectSocket, message.c_str(), message.size(), 0);

		Sleep(2000);

		char answer[DEFAULT_BUFLEN];
		iResult = recv(ConnectSocket, answer, DEFAULT_BUFLEN, 0);
		if (iResult < DEFAULT_BUFLEN - 1)
			answer[iResult] = '\0';
		else
			answer[DEFAULT_BUFLEN - 1] = '\0';

		if (iResult > 0) {
			cout << "процес сервера надіслав відповідь: " << answer << "\n";
			cout << "отримано байтів: " << iResult << "\n";
		}
		else if (iResult == 0)
			cout << "з'єднання з сервером закрито.\n";
		else
			cout << "recv не вдалося з помилкою: " << WSAGetLastError() << "\n";
	}

	closesocket(ConnectSocket);
	WSACleanup();

	cout << "процес клієнта завершує свою роботу!\n";
}