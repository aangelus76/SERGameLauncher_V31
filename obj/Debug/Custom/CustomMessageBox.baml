   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5   	   +    %SERGamesLauncher_V31.CustomMessageBox   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:SERGamesLauncher_V31     5   	     � Title$  Message��5   	   $	��249��5	   	   $	��400��5
   	    � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5   	   
������  � AllowsTransparency .  � WindowStartupLocation$ CenterOwner=�5   	   .��5      ��  �� CornerRadius$ 15��6   ���#474747._�5   
   � $��2q�6    � RowDefinitions 5      �� 5      $��40 �6    �� 5      �� 5      $��60 �6    .+�5      �� $��0��6   ���#171717$ 	12,12,0,0��67   ._�5      � .+�5      ��  �� Orientation$ 
Horizontal=�6!   $��Center=�6:   $��10,0,0,0q�6U   .+�5      �� -   5         ��Name$ 	iconImage��5       $��20��63   $��20��6>   $��0,0,8,0q�6J   �� 5      -   5        	 ��Name$	 titleTextBlock��5   $   $��Message��6<   
�������$��16(�6^   $
��Bold"�6l   5      �� 5!      -   5        
 ��Name$
 btnClose��5!         IconButtonStyle#��[  6/    � ToolTip$ Fermer��5"      $��Right=�6.   $��0,0,10,0q�6J   .��5$      �� -   5        $ 
closeImage��5$         ForceNormal#��[ 64   $��26��6Z   $��26��6e    � Triggers 5%      :� 5&       :� RoutedEvent$ Button.MouseEnter��6+   .[�5'   "   �� .��5(   &   �� .��5)   *   T�  �� 
TargetName$ 
closeImage��6H    �� TargetProperty$ Source�6k   ..�5*   .   i�  R� KeyTime$ 0:0:0��6E     	ForceOver R� Value# [ 6U   5+   +   5,   '   5-   #   5.      :� 5/      $ Button.MouseLeave��6+   .[�50   "   �� .��51   &   �� .��52   *   T� $ 
closeImage��6H   $ Source�6k   ..�53   .   i� $ 0:0:0��6E   # [ 6U   54   +   55   '   56   #   57      59      5:      5;      �� 5>      $��1��6   $
��Auto=�6(   $��Disabled=�6K   $��0,0,0,54q�6t   $��2��6�   .��5?      �� -   5        $	 messageTextBlock��5?      $��Message��5@      
�������$��14(�6>   $
��Wrap=�5A      $��20,15q�60   $��Center=�6?   $��Center=�5B      5C      � 5F      -   5         �Name$ 
buttonGrid��5F       $��1��6   $��
0,132,0,10q�64   $��2��6H   5H      5I      5J      5K      