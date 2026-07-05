PROJECT   := $(wildcard *.csproj)
CONFIG    := Release
OUT       := publish

RIDS := win-x64 win-arm64 linux-x64 linux-arm64 osx-x64 osx-arm64

PUBLISH_FLAGS := -c $(CONFIG) \
	--self-contained true \
	-p:PublishSingleFile=true \
	-p:IncludeNativeLibrariesForSelfExtract=true \
	-p:PublishTrimmed=false

.PHONY: all clean $(RIDS)

all: $(RIDS)

$(RIDS):
	dotnet publish $(PROJECT) -r $@ $(PUBLISH_FLAGS) -o $(OUT)/$@

winx64: win-x64
winarm64: win-arm64
linuxx64: linux-x64
linuxarm64: linux-arm64
osxx64: osx-x64
osxarm64: osx-arm64

clean:
	rm -rf $(OUT)
