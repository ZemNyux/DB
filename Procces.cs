#include <windows.h>
#include <iostream>
 
int main()
{
    STARTUPINFOA si = {0};           // структура з інформацією про вікно та stdin/stdout
    PROCESS_INFORMATION pi = {0};     // сюди запишеться інформація про створений процес
 
    si.cb = sizeof(STARTUPINFOA);     // обов’язково вказуємо розмір структури
 
    // Параметри, які найчастіше залишають NULL або 0
    LPSECURITY_ATTRIBUTES lpProcessAttributes  = NULL;   // 1
    LPSECURITY_ATTRIBUTES lpThreadAttributes   = NULL;   // 2
    BOOL                  bInheritHandles      = FALSE;  // 3
    DWORD                 dwCreationFlags      = 0;      // 4
    LPVOID                lpEnvironment        = NULL;   // 5
    LPCSTR                lpCurrentDirectory  = NULL;   // 6
 
    // Найголовніше — командний рядок
    char commandLine[] = "notepad.exe";                  // 7
 
    BOOL result = CreateProcessA(
        NULL,                       // 1. lpApplicationName           → повний шлях до exe або NULL
        commandLine,                // 2. lpCommandLine               → рядок з аргументами
        lpProcessAttributes,        // 3. 
        lpThreadAttributes,         // 4.
        bInheritHandles,            // 5.
        dwCreationFlags,            // 6.
        lpEnvironment,              // 7.
        lpCurrentDirectory,         // 8.
&si,                        // 9. lpStartupInfo
&pi                         //10. lpProcessInformation
    );
 
    if (result == 0)
    {
        DWORD err = GetLastError();
        std::cout << "CreateProcessA failed. Error code: " << err << "\n";
        if (err == 2)
            std::cout << "(файл не знайдено)\n";
        else if (err == 3)
            std::cout << "(шлях не знайдено)\n";
        else if (err == 5)
            std::cout << "(немає доступу)\n";
    }
    else
    {
        std::cout << "Процес успішно створено!\n";
        std::cout << "PID: " << pi.dwProcessId << "\n";
        std::cout << "TID: " << pi.dwThreadId  << "\n";
 
        // Чекаємо завершення процесу (необов’язково)
        // WaitForSingleObject(pi.hProcess, INFINITE);
 
        // Закриваємо хендли
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    }
 
    system("pause");
    return 0;
}
