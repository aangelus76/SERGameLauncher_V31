   M S B A M L   `   `   `  ����O  KSERGamesLauncher_V31, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:SERGamesLauncher_V31SERGamesLauncher_V31  5      -    'SERGamesLauncher_V31.PathConfigsControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      4local"clr-namespace:SERGamesLauncher_V31     5      .��5
      �   � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $.��(Configuration des chemins d'applications��6!   
�������$��24(�5   '   $
��Bold"�65   $��Center=�5      $��	0,20,0,20q�61   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ pathsDataGrid��5       $��1��6   
������    RowBackground �#1E1E1E   AlternatingRowBackground �#242424   RowHeaderWidth$ 0Q�5      $��0q�5         GridLinesVisibility$ 
Horizontal=�5         HorizontalGridLinesBrush �#333333   AutoGenerateColumns .     CanUserAddRows .  	   CanUserDeleteRows	 .  
   CanUserReorderColumns
 .     CanUserResizeRows .     CanUserSortColumns .    SelectionMode$ Single=�5%         HeadersVisibility$ Column=�5&      "   VerticalScrollBarVisibility$
 Auto=�5'      $   HorizontalScrollBarVisibility$
 Auto=�5(      $
��30,0q�5)       � 	Resources 5+      *  $System.Windows.Controls.DataGridCell �� 
TargetType �� Property �� Value �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader��%�  '        '  �    �� 5-      "  6   .�5.      ��  � ��
 ���� 63   �� 5/       � ��$ 0q�68   �� 50       � ��$ 5q�60   �� 51       � ��$
 Bold"�63   �� 52       � ��$ 14(�61   �� 53       � �� 54      �� 55      "  6.   .R�56   "   �� #��z��6)   $��0q�6S   ._�57   &   �� $��Center=�67   $
��10,5q�6R   58   #   59      5;      5<      �� 5?      "  6   .�5@      ��  � ��
 ���� 63   �� 5A       � �� �#26853163   �� 5B       � ��$ 16(�61   �� 5C       � ��$
 Bold"�63   �� 5D       � ��$
 10,5q�60   �� 5E       � ��$ 0,0,0,1q�68   �� 5F       � �� �#33333364   �� 5G       � ��$
 Left=�6C   5H      5I         Columns 5K      0  *System.Windows.Controls.DataGridTextColumn  5M      ,  &System.Windows.Controls.DataGridColumn   Header$ 
PLATEFORME��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$	 120 6Z   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *PlatformName+   ElementStyle 5N      �� 5O      " ��6    .�5P      ��  � ��
 �����6;   �� 5Q       � ��$ Center=�6B   �� 5R       � ��$
 Bold"�6;   �� 5S       � ��$ 14(�69   5T      5V        5Y      $
 TYPE��6%   $ 80 63    5Z      �� 5[       �� Path$ IsUrl�6"      BoolToPathTypeConverter �� 	Converter# [  6/    5]      �� 5^      " ��6    .�5_      ��  � ��
 �����6;   �� 5`       � ��$ Center=�6B   �� 5a       � ��$
 Bold"�6;   �� 5b       � ��$ 14(�69   5c      5e        5h      $ CHEMIN��6%   $ * 6N    65   �� *Path+ 5i      �� 5j      " ��6    .�5k      ��  � ��
 �����6;   �� 5l       � ��$ Center=�6B   �� 5m       � ��$
 Bold"�6;   �� 5n       � ��$ 14(�69   �� 5o       � ��$ CharacterEllipsis=�6=   �� 5p       �@�ToolTip 68   �� *Path+5q      5s        5v      $ 	ARGUMENTS��6%   $	 150 6\    68   �� *LaunchArguments+ 5w      �� 5x      " ��6    .�5y      ��  � ��
 �����6;   �� 5z       � ��$ Center=�6B   �� 5{       � ��$
 Bold"�6;   �� 5|       � ��$ 14(�69   �� 5}       � ��$ CharacterEllipsis=�6=   �� 5~       �@�ToolTip 68   �� *LaunchArguments+5      5�      4  .System.Windows.Controls.DataGridTemplateColumn  5�      $ 80 6)      CellTemplate 5�      �� 5�      .R�5�      ��  �� Orientation$ 
Horizontal=�6)   $��Center=�6B   .+�5�   "   �� -   5          IconButtonStyle#��[ 5�   )    � ToolTip$ Modifier ce chemin��5�   )   $��24��5�   )   $��24��64   $	��4,0q�6@   
  � Tag  5�   )   �� *Id+.��5�   &   F� �����@(��o � 6� �0F ׀�� PE  �� �� Q@KL ���(\+@W��C |�G�z-@ �� �� � 	= 0� ��uA 0�, �@KL  -1 �ЪM  -1��K� �`+�p=
ף�$@�" �� �� Ѱ)�Y� W@(]���(\@  L  L ����  �Q �\ gffff�1@Q��` ������-@� � @6� ��M�  i� � $� ��� �@?�  z �p � ��[ р��5� 	���#268531! �� Stretch$! Uniform=�5�   :   $��18��6L   $��18��6W   5�   #   �� 5�   "   -   5        #��[ 5�   )   $��24��5�   )   $��24��64   $	��4,0q�6@   $ Supprimer ce chemin��5�   )     5�   )   �� *Id+.��5�   &   F� �������!��j � $� ��j � $� ��[ ׀��  	=  �� �� р��  	= � z ��[  �� �� � z ��j �@KL ��j �@KL @T�  р�[ @T� р�[  -1�@T� ��^ ��- ��- ���� ��^׀� -1 ��- ��- р�@T� ���!@T� ���!��j  ���� ��[ р�� ��[ р�� ��j р�� ��j �@f -1� $� @o@�� z @o@���j  -1����j @T� �@f@T� ���� ����ا @f ��ا �ا �@T� �ا  �@T� @f׀�� �� ���� ������ @f ���� �ا �@]� �ا  �@]� @f׀�� �� 	���#db1f32$! Uniform=�5�   :   $��18��6L   $��18��6W   5�   #   5�      5�      5�      5�      � 5�   
   $��2��6   $��30,20q�6   .+�5�      �� -   5        $��Ajouter un chemin��5�        LaunchButtonStyle#��[ 5�      $	��170��5�      $��40��6!   $
��Left=�6-   ���#2685315�      5�      5�      5�      