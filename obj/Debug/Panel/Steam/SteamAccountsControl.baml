   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5      /    )SERGamesLauncher_V31.SteamAccountsControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      4local"clr-namespace:SERGamesLauncher_V31     5      .��5	      �   � RowDefinitions  5
   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $%��Configuration des comptes steam��6!   
�������$��24(�5   '   $
��Bold"�65   $��Center=�5      $��	0,20,0,20q�61   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ accountsDataGrid��5       $��1��6   
������    RowBackground �#1E1E1E   AlternatingRowBackground �#242424   RowHeaderWidth$ 0Q�5      $��0q�5         GridLinesVisibility$ 
Horizontal=�5         HorizontalGridLinesBrush �#333333   AutoGenerateColumns .     CanUserAddRows .  	   CanUserDeleteRows	 .  
   CanUserReorderColumns
 .     CanUserResizeRows .     CanUserSortColumns .    SelectionMode$ Single=�5$         HeadersVisibility$ Column=�5%      "   VerticalScrollBarVisibility$
 Auto=�5&      $   HorizontalScrollBarVisibility$ Hidden=�5'      $
��30,0q�5(         
IsReadOnly .    RowStyle 5,      �� 5-       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6    �� Triggers 5.      �� 5/       �� Binding 6&   �� *IsCurrentComputer+ �� Value True6L   .a�50      ��  �� Property � �� �� Value �#2A2A2A6;   51      53       � 	Resources 56      *  $System.Windows.Controls.DataGridCell �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader��%�  '        '  �    �� 58      "  6   .�59      ��  � ��
 ���� 63   �� 5:       � ��$ 0q�68   �� 5;       � ��$ 5q�60   �� 5<       � ��$
 Bold"�63   �� 5=       � ��$ 14(�61   �� 5>       � �� 5?      �� 5@      "  6.   .R�5A   "   �� #��z��6)   $��0q�6S   ._�5B   &   �� $��Center=�67   $
��10,5q�6R   5C   #   5D      5F      5G      �� 5J      "  6   .�5K      ��  � ��
 ���� 63   �� 5L       � �� �#26853163   �� 5M       � ��$ 16(�61   �� 5N       � ��$
 Bold"�63   �� 5O       � ��$
 10,5q�60   �� 5P       � ��$ 0,0,0,1q�68   �� 5Q       � �� �#33333364   �� 5R       � ��$
 Left=�6C   5S      5T         Columns 5V      0  *System.Windows.Controls.DataGridTextColumn  5X      ,  &System.Windows.Controls.DataGridColumn   Header$ POSTE��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 64   1  +System.Windows.Controls.DataGridBoundColumn   Binding 5Y      �� 5Z       �� Path$ 	PosteName�6"       ElementStyle  5\      �� 5]      " ��6    .�5^      ��  � ��$ Center=�6B   �� 5_       � ��$
 Bold"�6;   �� 5`       � ��$ 14(�69    5a      �� 5b   "    6.   �� *IsCurrentComputer+ True6T   .a�5c   &   ��  � �� �#db1f326C   5d   #   �� 5e   "    6.   �� *IsCurrentComputer+	 False6T   .a�5f   &   ��  � ��
 �����6C   5g   #   5i      5k        5n      $ IDENTIFIANT��6%   $ * 6W    6:   �� *
Username+  5o      �� 5p      " ��6    .�5q      ��  � ��
 �����6;   �� 5r       � ��$ Center=�6B   �� 5s       � ��$
 Bold"�6;   �� 5t       � ��$ 14(�69   �� 5u       � ��$
 None=�6=   �� 5v       � ��$ NoWrap=�6=   �� 5x       �@J�IsHitTestVisible . 6A   �� 5y       � |� . 6:   5z      5|      4	  .System.Windows.Controls.DataGridTemplateColumn	  5      $ MOT DE PASSE��6)   $ * 6?   ! 	  CellTemplate! 5�      �� 5�      .R�5�      �� $��Courier New)�5�   $   
�������$��Center=�5�   $   $
��Bold"�5�   $   $��14(�5�   $   
������ $��0q�5�   $   " �� 
IsReadOnly" . # J� IsHitTestVisible# . ��5�   &   �� *PasswordDisplay+$ �� Mode$$ OneWay=�5�   $   5�      5�      	  5�      $ 80 6)   ! 5�      �� 5�      .R�5�      �� % �� Orientation$% 
Horizontal=�6)   $��Center=�6B   .+�5�   "   �� -   5           IconButtonStyle#��[  5�   )   & � ToolTip$& Modifier ce compte��5�   )   $��24��5�   )   $��24��64   $	��4,0q�6@   
' � Tag' 5�   )   �� *Id+.��5�   &   F� �����@(��o � 6� �0F ׀�� PE  �� �� Q@KL ���(\+@W��C |�G�z-@ �� �� � 	= 0� ��uA 0�, �@KL  -1 �ЪM  -1��K� �`+�p=
ף�$@�" �� �� Ѱ)�Y� W@(]���(\@  L  L ����  �Q �\ gffff�1@Q��` ������-@� � @6� ��M�  i� � $� ��� �@?�  z �p � ��[ р��5� 	���#268531( �� Stretch$( Uniform=�5�   :   $��18��6L   $��18��6W   5�   #   �� 5�   "   -   5        #��[  5�   )   $��24��5�   )   $��24��64   $	��4,0q�6@   $& Supprimer ce compte��5�   )   ' 5�   )   �� *Id+.��5�   &   F� �������!��j � $� ��j � $� ��[ ׀��  	=  �� �� р��  	= � z ��[  �� �� � z ��j �@KL ��j �@KL @T�  р�[ @T� р�[  -1�@T� ��^ ��- ��- ���� ��^׀� -1 ��- ��- р�@T� ���!@T� ���!��j  ���� ��[ р�� ��[ р�� ��j р�� ��j �@f -1� $� @o@�� z @o@���j  -1����j @T� �@f@T� ���� ����ا @f ��ا �ا �@T� �ا  �@T� @f׀�� �� ���� ������ @f ���� �ا �@]� �ا  �@]� @f׀�� �� 	���#db1f32$( Uniform=�5�   :   $��18��6L   $��18��6W   5�   #   5�      5�      5�      5�      � 5�   
   $��2��6   $��30,20q�6   ) � ColumnDefinitions) 5�      �� 5�      $��* �6#   �� 5�      $��* �6#   .+�5�      �� -   5        $��0��5�      $��Ajouter un compte��6%     LaunchButtonStyle#��[ 5�      $	��170��5�      $��40��6!   $
��Left=�6-   ���#2685315�      �� 5�      -   5        * ��Name$* btnShowPasswords��5�   %   $��1��6   $��Voir les MDP��6?   #��[ 5�      $	��170��5�      $��40��6!   $��Right=�6-   ���#821e1e5�      5�      5�      5�      