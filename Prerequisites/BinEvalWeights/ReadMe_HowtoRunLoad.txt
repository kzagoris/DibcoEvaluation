The executable generates two .dat files containing
the normalized distance weights for pseudo-Recall/Precision based on paper:

K. Ntirogiannis, B. Gatos and I. Pratikakis
"Performance Evaluation Methodology for Historical Document Image Binarization"
IEEE Trans. Image Proc., vol.22, no.2, pp. 595-609, Feb. 2013.

The generated .dat files shall be used with the DIBCO13 Evaluation tool.
However, information is provided so that it can be loaded by any other program.

A. Input/Output
Input:  Filename of the binary ground-truth image (supported formats of OpenCV ver.1).
Output: *_RWeights.dat and *_PWeights.dat

B. File Format
File format of generated .dat files:
value(double) space(char)
(i.e. fprintf(file,"%f %c",value,space), where space=' ')

-- Values are generated starting from the upper left image corner following horizontal scan.

C. Example Loading Procedure

C1. c style:
file=fopen("RWeights.dat","r");
char space; char value[9]; double valD;
for(int y=0;y<ImageHeight;y++)
for(int x=0;x<ImageWidth;x++)
	{fscanf(file,"%s%c",value,&space); valD=value.toDouble(); etc...} --> Weights(x,y)=valD;

Notice that "value" may need modification by alternating "." to "," according to the system decimal format. 

C2. Matlab style:

s=size(imread(image));
x=s(1); (opposite form c style, image widht is image height and vice versa).
y=s(2);

fid1=fopen('RWeigts.dat','r');
RWeights=fscanf(fid1,'%f%*c',[y x]);
RWeights=RWeights';

-Access by RWeights(x,y).

D. Calculating pseudo-Recall/Precision

True Positives:

TP = TP + 1; -->  Recall and Precision
TPwp = TPwp + 1 + PWeights(i,j); --> pseudo-Precision
TPwr = TPwr + RWeights(i,j); --> pseudo-Recall

False Positives:

FP = FP + 1; --> Precision
FPwp = FPwp + 1 + PWeights(i,j); --> pseudo-Precision

False Negatives:

FN = FN + 1; --> Recall
FNwr = FNwr + RWeights(i,j); --> pseudo-Recall

pseudo-Recall/Precision

w_Precision = (TPwp) / (FPwp+TPwp);
w_Recall = (TPwr) / (TPwr+FNwr);

E. CAUTION: Do not forget to compare your results with the DIBCO 2013 evaluation tool to assure correct loading.

F. Exaple run:
C:\DIBCO2013\Public\Exe>BinEvalWeights.exe PR07_estGT.tiff

Starting 7 stages procedure:
1. Loading GT and CC Detection
2. Skeleton and Contour
3. Distance Weights for Recall
4. Loading Inverted GT and CC Detection
5. Skeleton
6. Distance Weights for Precision
7. Releasing Mem and Terminating


