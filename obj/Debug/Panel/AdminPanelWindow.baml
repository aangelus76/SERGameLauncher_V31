   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5   	   +    %SERGamesLauncher_V31.AdminPanelWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:SERGamesLauncher_V31     5   	     � Title$  Panneau d'administration��5   	   $	��650��5	   	   $
��1100��5
   	    � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5   	   
������  � AllowsTransparency .  � WindowStartupLocation$ CenterScreen=�5   	   .��5      �� 
������ $��5q�6'    �� CornerRadius$ 15��6;   
������ ._�5   
   �� ���#333333$��1q�6'   $ 15��6;      BackgroundBrush#��[  6M    J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5         
BlurRadius$ 10Q�6#      	Direction$ 0Q�63   	   ShadowDepth$		 0.5Q�6A   
   Color$
 #d62929��6S      Opacity$	 0.3Q�6c   ._�5      � $��2q�6    � RowDefinitions 5      �� 5      $��40 �6$   �� 5      $��* �6$   .+�5      �� $��0��6   ���#171717$ 	12,12,0,0��6;   ._�5      � .+�5       ��  �� Orientation$ 
Horizontal=�6%   $��Center=�6>   $��10,0,0,0q�6Y   .+�5!      �� $>��8/SERGamesLauncher_V31;component/Images/IconeLauncher.ico��6$   $��20��6f   $��20��6q   $��0,0,8,0q�6}   �� 5"      $)��#Panneau d'administration SER-Games ��6(   
�������$��16(�6f   $
��Bold"�6t   �� 5#      $��V.3.1��6(   
�������$��14(�6H   $��Center=�6V   5$      �� 5&      $ 
Horizontal=�6%   $��Right=�6>   $��0,0,10,0q�6Z   .+�5(      �� -   5         ��Name$ btnMinimize��5(   %     IconButtonStyle#��[ 6:    � ToolTip$ Minimiser la fenêtre��5)   !   .��5+   "   �� -   5         ��Name$ 	miniImage��5+   (     
MiniNormal#��[ 6;   $��26��6`   $��26��6k    � Triggers 5,   "   :� 5-   &    :� RoutedEvent$ Button.MouseEnter��63   .[�5.   *   �� .��5/   .   �� .��50   2   T�  �� 
TargetName$ 	miniImage��6P    �� TargetProperty$ Source�6r   ..�51   6   i�  R� KeyTime$ 0:0:0��6M     MiniOver R� Value# [ 6]   52   3   53   /   54   +   55   '   :� 56   &   $ Button.MouseLeave��63   .[�57   *   �� .��58   .   �� .��59   2   T� $ 	miniImage��6P   $ Source�6r   ..�5:   6   i� $ 0:0:0��6M   # [ 6]   5;   3   5<   /   5=   +   5>   '   5@      �� 5C      -   5        $ btnClose��5C   %   #��[ 67   $ Fermer��5D   !   .��5F   "   �� -   5        $ 
forceImage��5F   (     ForceNormal#��[ 6<   $��26��6b   $��26��6m    5G   "   :� 5H   &   $ Button.MouseEnter��63   .[�5I   *   �� .��5J   .   �� .��5K   2   T� $ 
forceImage��6P   $ Source�6s   ..�5L   6   i� $ 0:0:0��6M     	ForceOver# [ 6]   5M   3   5N   /   5O   +   5P   '   :� 5Q   &   $ Button.MouseLeave��63   .[�5R   *   �� .��5S   .   �� .��5T   2   T� $ 
forceImage��6P   $ Source�6s   ..�5U   6   i� $ 0:0:0��6M   # [ 6]   5V   3   5W   /   5X   +   5Y   '   5[      5\      5]      5^      � 5a      $��1��6    � ColumnDefinitions 5b      �� 5c      $	��250 �6+   �� 5d      $��* �6+   .+�5h      �� $��0��6   ���#242424$ 0,0,0,12��6B   ._�5i      �� $��0,15,0,0q�6%   .+�5j      �� -   5        $ btnSteamAccounts��5j   %     PlatformButtonStyle#��[ 6?   .��5k   "   �� $��Comptes Steam��6,   
�������$��18(�6T   $
��10,0q�6b   $��Center=�6p   5l      �� 5n      -   5        $ btnPlatformVisibility��5n   %   #��[ 6D   .��5o   "   �� $��Visibilité des boutons��6,   
�������$��18(�6]   $
��10,0q�6k   $��Center=�6y   5p      �� 5r      -   5        $ btnFolderPermissions��5r   %   #��[ 6C   .��5s   "   �� $��Permissions dossiers��6,   
�������$��18(�6[   $
��10,0q�6i   $��Center=�6w   5t      �� 5v      -   5        $ btnAppPaths��5v   %   #��[ 6:   .��5w   "   �� $��Chemins d'applications��6,   
�������$��18(�6]   $��Center=�6k   $	��186��6�   5x      �� 5z      -	   5        $ btnProcessRestrictions��5z   %   #��[ 6E   .��5{   "   �� $��Restrictions d'âge��6,   
�������$��18(�6Y   $��Center=�6g   $	��186��6�   5|      5}      5~      �� 5�      $��1��6   ���#1E1E1E$ 0,0,12,0��6B   ._�5�      �� -
   5         ��Name$ contentPresenter��5�   +   ��6E   ��  �� ElementName$ defaultMessage�� � 	Resources 5�        defaultMessage��%�   &	       �� 5�      $+��%Sélectionnez une option dans le menu��5�      
�������$��24(�5�   '   $��Center=�5�      $��Center=�5�      5�      5�      5�      5�      5�      5�      5�      