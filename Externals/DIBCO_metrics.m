function DIBCO_metrics(fn,fn2,RName,PName,m,blck)
% fn = 'PR_GT.tiff';
% fn2 = 'PR_bin.bmp';
% RName = 'PR_RWeights.dat';
% PName = 'PR_PWeights.dat';

% fn1 = ground truth (gt) image
% fn2 = filename of processed binary image
% m = the size of the weight normalized matrix ...
%  ... authors recommend 5 (5x5) for DRD
% blck = the size of the non-Uniform blocks to be measured ...
%  ... auhtors recomend 8 (8x8) for DRD

nar = nargin;

if nar == 4
    m = 5;
    blck = 8;
elseif nar ~= 6
    error( 'Usage: DIBCO_metrics <GT image> <Binarized image for evaluation> <Recall Weights> <Precision Weights>' )
end

%Read GT image
z=imread(fn);%BG=1 white
if max(z(:))>1
    z = logical(z);
    show_error = 'non 1-bit, continuing';
end
z=imcomplement(z);%BG=0 black
%z = z(:,:,1);

%Dimensions
s=size(z);
x=s(1);
y=s(2);

%Read Bin image
zb=imread(fn2);%BG=1 white
sb=size(zb);
if size(sb,2)>2
    zb=rgb2gray(zb);
end

if max(zb(:))>1
    zb = logical(zb);
    show_errorB = 'non 1-bit, continuing';
end
zb=imcomplement(zb);%BG=0 black
x=size(z,1);
y=size(z,2);

%Load R(ecall)Weights and P(recision)Weights

fid1=fopen(RName,'r');
RWeights=fscanf(fid1,'%f%*c',[y x]);
RWeights=RWeights';
fclose(fid1);

fid2=fopen(PName,'r');
PWeights=fscanf(fid2,'%f%*c',[y x]);
PWeights=PWeights';
fclose(fid2);

%True/False Positives/Negatives
TP=0;
FP=0;
FN=0;

%True/False Positives/Negatives WPrecision
TPwp=0;
FPwp=0;

%True/False Positives/Negatives WRecall
TPwr=0;
FNwr=0;

%weight matrix for DRD
wo=zeros(m);
wo(round(m/2),round(m/2))=1;
wm=bwdist(wo);
%to avoid devision by zero
wm(round(m/2),round(m/2))=1;
wm=1./wm;
wm(round(m/2),round(m/2))=0;

%weight normalized matrix for DRD
wnm=zeros(m);
sm=sum(wm(:));
wnm=wm/sm;

%for DRD
sum_DRDk=0;

%Flipped pixels
retro_FL = 0;

%Error Image
%EI1 = zeros(x,y,3);
%EI1 = uint8(EI1);
%EI1 = EI1 + 255;

%Metrics Calculation

%for each pixel
for i=1:x
    for j=1:y
            
            %Precision
            if zb(i,j)~=0 %Fg Bin Res
                if z(i,j)~=0 %Fg GT
                    TP = TP + 1;
                    TPwp = TPwp + 1 + PWeights(i,j);
                    TPwr = TPwr + RWeights(i,j);
                    %Gray painting
                    %EI1(i,j,1) = 160;
                    %EI1(i,j,2) = 160;
                    %EI1(i,j,3) = 160;
                else %Bg of GT
                        FP = FP + 1;
                        FPwp = FPwp + 1 + PWeights(i,j);
                        %Blue painting
                        %EI1(i,j,1) = 0;
                        %EI1(i,j,2) = 0;
                end
            end
            
            %Recall
            if z(i,j)~=0 %Fg GT
                    if zb(i,j)~=0 %Fg Bin Res
                    else
                        FN = FN + 1;
                        FNwr = FNwr + RWeights(i,j);
                        %Green painting
                        %EI1(i,j,1) = 0;
                        %EI1(i,j,3) = 0;
                    end
            end
            
            %Other metrics
            if zb(i,j)~=z(i,j)  
                retro_FL = retro_FL + 1;
                DRDk = DRDcalc(i,j,z,zb,wnm,m);
                sum_DRDk = sum_DRDk + DRDk;
            end 
    end
end
   
%Calculate Precision
retro_Precision = (TP) / (FP+TP);
 
%Calculate Recall
retro_Recall = (TP) / (TP+FN);

%Calculate FMeasure
retro_FM = 200*retro_Recall*retro_Precision/(retro_Recall + retro_Precision);

%Calculate New
w_Precision = (TPwp) / (FPwp+TPwp);
w_Recall = (TPwr) / (TPwr+FNwr);
w_FM = 200*w_Recall*w_Precision/(w_Recall + w_Precision);

%Calculate PSNR
retro_PSNR = 10*log10((x*y)/(retro_FL));

%Calculate Nonuniform Blocks NUBN for DRD
xb = x/blck;
xb = floor(xb);
yb = y/blck;
yb = floor(yb);

sum_NUBN=0;

for i=1:xb
    for j=1:yb
        nubn_b = NUBNcalc(z,i,j,blck);
        sum_NUBN = sum_NUBN + nubn_b;
    end
end

%Calculate DRD
drd=sum_DRDk/sum_NUBN;

metrics(1)=retro_FM;
metrics(2)=w_FM;
metrics(3)=retro_PSNR;
metrics(4)=drd;
metrics(5)=100*retro_Recall;
metrics(6)=100*retro_Precision;
metrics(7)=100*w_Recall;
metrics(8)=100*w_Precision;

fprintf(['F-Measure;pseudo F-Measure (Fps);PSNR;DRD;Recall;Precision;pseudo-Recall (Rps);pseudo-Precision (Pps)' newline])
fprintf([num2str(metrics(1)) ';' num2str(metrics(2)) ';' num2str(metrics(3)) ';' num2str(metrics(4)) ';' num2str( metrics(5)) ';' num2str(metrics(6)) ';' num2str(metrics(7)) ';' num2str(metrics(8)) newline])

% fprintf( '\n' )
% fprintf( [ 'F-Measure\t\t:\t' num2str( metrics(1) ) '\n' ] )
% fprintf( [ 'pseudo F-Measure (Fps)\t:\t' num2str( metrics(2) ) '\n' ] )
% fprintf( [ 'PSNR\t\t\t:\t' num2str( metrics(3) ) '\n' ] )
% fprintf( [ 'DRD\t\t\t:\t' num2str( metrics(4) ) '\n' ] )
% fprintf( [ 'Recall\t\t\t:\t' num2str( metrics(5) ) '\n' ] )
% fprintf( [ 'Precision\t\t:\t' num2str( metrics(6) ) '\n' ] )
% fprintf( [ 'pseudo-Recall (Rps)\t:\t' num2str( metrics(7) ) '\n' ] )
% fprintf( [ 'pseudo-Precision (Pps)\t:\t' num2str( metrics(8) ) '\n' ] )

% fprintf( '\n%.5f %.5f %.5f %.5f %.5f %.5f %.5f %.5f\n' , ...
%     metrics(1) , metrics(2) , metrics(3) , metrics(4) , metrics(5) , ...
%     metrics(6) , metrics(7) , metrics(8) )

%dotloc=findstr(fn2,'.');
%image_writ=fn2(1:dotloc-1);
%image_wrt1=strcat(image_writ,'_EI.bmp');

%imwrite(EI1,image_wrt1);

clear zb;
%clear EI1;

end

function ret=DRDcalc(i,j,f,g,wnm,m)

%Dimensions
sf=size(f);
xf=sf(1);
yf=sf(2);

%Value of the k-th fliiped pixel
% at the processed image g
value_gk=g(i,j);

%Construct mxm Block Bk of gt image (f)
%and measure the Difference matrix Dk
Bk = zeros(m);
Dk = zeros(m);
h = floor(m/2);
for x=0:m-1
    for y=0:m-1
        if(i-h+x<1 || j-h+y<1 || i-h+x>xf || j-h+y>yf)
            Bk(x+1,y+1) = value_gk;
        else
            Bk(x+1,y+1) = f(i-h+x,j-h+y); 
        end
        Dk(x+1,y+1) = abs(Bk(x+1,y+1)-value_gk); 
    end
end

%The distortion DRDk for the k-th pixel
%of the processed image g is "ret"
DRDk = Dk.*wnm;
ret = sum(DRDk(:));

end

function retb=NUBNcalc(f,ii,jj,blck)

startx = (ii-1)*blck + 1;
endx = ii*blck;

starty = (jj-1)*blck + 1;
endy = jj*blck;

check_prv = -2;

retb = 0;

for xx=startx:endx
    for yy=starty:endy
        check = f(xx,yy);
        if(check_prv<0)
            check_prv=check;
        else
            if(check~=check_prv)
                retb = 1;
                break;
            end
        end
    end
    if(retb~=0)
        break;
    end
end

end
