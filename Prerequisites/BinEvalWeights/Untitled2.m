clc
clear
close all

gtfolder = [ pwd '\gt\' ];

for i = 1:10,
%     disp( i );
    gtfile = [ gtfolder num2str( i ) '_gt.bmp' ];
    I = imread( gtfile );
    if size( I , 3 ) ~= 1,
        I = I(:,:,1);
    end
    system( [ 'BinEvalWeights.exe ' gtfile ] )
    
    filename = num2str( i );
    figure(i)
    subplot( 1 , 3 , 1 )
    imshow( I )
    title( [gtfolder filename '.bmp' ] )
    
    subplot( 1 , 3 , 2 )
    A = dlmread( [gtfolder filename '_gt' '_PWeights.dat' ] );
    s = size( I );
    sz = [ s(2) s(1) ];
    IA = reshape( A , sz )';
    imshow( IA / max( IA(:) ) );
    title( [gtfolder filename '_gt' '_PWeights.dat' ] )
    
    subplot( 1 , 3 , 3 )
    A = dlmread( [gtfolder filename '_gt' '_RWeights.dat' ] );
    IA = reshape( A , sz )';
    imshow( IA / max( IA(:) ) );
    title( [ filename '_gt' '_RWeights.dat' ] )
    
end

