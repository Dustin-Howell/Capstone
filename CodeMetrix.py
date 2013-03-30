import os
from os.path import join, getsize, splitext

root= os.getcwd()

fileExtensionCounts= {}
byteSum= 0
fileExts= ['.cs', '.xaml', '.png', '.bmp', '.txt', '.spritefont', '.wmv', '.fbx', '.x', '.dds', '.nsi', '.iss', '.jpg', '.py']
ignoredFolders= ['.git', 'packages', 'bin', 'obj']

for root, dirs, files in os.walk(root):
	byteSum+= sum(getsize(join(root, file)) for file in files)
	for ext in filter(lambda x: x in fileExts, map(lambda y: splitext(y)[1].lower(), files)):
		try:
			fileExtensionCounts[ext]+= 1
		except KeyError:
			fileExtensionCounts[ext]= 1
	for folder in ignoredFolders:
		if folder in dirs:
			dirs.remove(folder)

for key in sorted(fileExtensionCounts.keys()):
	print key + " = " + str(fileExtensionCounts[key])

print "Total # of files: " + str(sum(fileExtensionCounts.values()))

print
print "Total relevant file size:"

abbreviations= ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'Insane Bytes!']
displayedSize= float(byteSum)
for abbrev in abbreviations:
	if len(str(int(displayedSize))) <= 3:
		print str(displayedSize) + " " + abbrev
		break
	else:
		displayedSize/= float(1000)
else:
	print "This is just way too big."
