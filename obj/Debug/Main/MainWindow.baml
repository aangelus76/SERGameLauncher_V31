   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5   	   %    SERGamesLauncher_V31.MainWindow   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:SERGamesLauncher_V31     5   	   -   5        '   !SERGamesLauncher_V31.CustomWindow   Name$  window��5   	    � Title$ Launcher SER-Games V.3.1��5	   	   $	��800��5
   	   $
��1250��5   	    � WindowStartupLocation$ CenterScreen=�5   	   .��5      �  � ColumnDefinitions 5   
   �� 5      $	��300 �6   �� 5      $��* �6   .+�5   
   �� $��0��6      LeftPanelStyle#��[  6!   ._�5      � .+�5      �� $��0,15,0,0q�6   .+�5      �� -   5         ��Name$ btnReglement��5        PlatformButtonStyle#��[ 63   .��5      ��  �� Orientation$ 
Horizontal=�6%   $��Center=�6>   .+�5      �� $>��8/SERGamesLauncher_V31;component/Images/IconeLauncher.ico��6$   $��50��6f   $��50��6q   �� 5      $��
Règlement��6(   
�������$��20(�6L   $
��15,0q�6Z   $��Center=�6h   5      5      �� 5       -   5        $ btnSteam��5       #��[ 6/   
 � Tag$ Steam��6y   .��5!      �� $ 
Horizontal=�6%   $��Center=�6>   .+�5"      ��   	SteamLogo#��[ 6$   $��50��6H   $��50��6S   �� 5#      $��Steam��6(   
�������$��20(�6H   $
��15,0q�6V   $��Center=�6d   5$      5%      �� 5'      -   5        $ btnEpic��5'      #��[ 6.   $
 Epic��6x   .��5(      �� $ 
Horizontal=�6%   $��Center=�6>   .+�5)      ��   EpicLogo#��[ 6$   $��50��6G   $��50��6R   �� 5*      $��
Epic Games��6(   
�������$��20(�6M   $
��15,0q�6[   $��Center=�6i   5+      5,      �� 5.      -   5        $ btnCrazy��5.      #��[ 6/   $ Crazy��6y   .��5/      �� $ 
Horizontal=�6%   $��Center=�6>   .+�50      ��   	CrazyLogo#��[ 6$   $��50��6H   $��50��6S   �� 51      $��
CrazyGames��6(   
�������$��20(�6M   $
��15,0q�6[   $��Center=�6i   52      53      �� 55      -   5        $ 	btnRoblox��55      #��[ 60   $ Roblox��6z   .��56      �� $ 
Horizontal=�6%   $��Center=�6>   .+�57      ��   
RobloxLogo#��[ 6$   $��50��6I   $��50��6T   �� 58      $��Roblox��6(   
�������$��20(�6I   $
��15,0q�6W   $��Center=�6e   59      5:      �� 5<      -   5        $ btnBGA��5<      #��[ 6-   $	 BGA��6w   .��5=      �� $ 
Horizontal=�6%   $��Center=�6>   .+�5>      ��   BGALogo#��[ 6$   $��50��6F   $��50��6Q   �� 5?      $��BoardGameArena��6(   
�������$��20(�6Q   $
��15,0q�6_   $��Center=�6m   5@      5A      �� 5C      -   5        $ btnXbox��5C      #��[ 6.   $
 Xbox��6x   .��5D      �� $ 
Horizontal=�6%   $��Center=�6>   .+�5E      ��   XboxLogo#��[ 6$   $��50��6G   $��50��6R   �� 5F      $��
Xbox Games��6(   
�������$��20(�6M   $
��15,0q�6[   $��Center=�6i   5G      5H      5I      �� 5L      $��Bottom=�6   $��Center=�68   $��0,0,0,20q�6U   .+�5M      �� -	   5         ��Name$ txtUserName��5M       $��Utilisateur : John Doe��65   ���#bbbdbb$��14(�5N   5   $��Center=�6C    �� TextAlignment$ Center=�5O       $��0,0,0,5q�67   �� 5P      -
   5        $ 
txtUserAge��5P       $��Âge : 3 ans��64   ���#bbbdbb$��14(�5Q   5   $��Center=�6C   $ Center=�5R       5S      5T      5U      �� 5X   
   -   5        	 ��Name$	 contentContainer��5X   K   $��1��6     MainContentStyle#��[ 6!   5Z      5[      5\      