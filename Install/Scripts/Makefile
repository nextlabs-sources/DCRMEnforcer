ifeq ($(NLBUILDROOT),)
  $(error NLBUILDROOT undefined.)
endif

ifeq ($(NLEXTERNALDIR),)
  $(error NLEXTERNALDIR undefined.)
endif

include $(NLBUILDROOT)/build/Makefile.ver

PRODUCT_DIR_NAME=SharepointOnlineEnforcer
VERSION_STR=$(VERSION_MAJOR_SPOE).$(VERSION_MINOR_SPOE)
ISM_TEMPLATE_FILE_NAME=SharepointOnlineEnforcer.ism
ISM_RELEASE=msi
ISM_PRODUCT_CONFIG64=64bit
MSI_FILE64=SharepointOnlineEnforcer.exe
TARGETMSI_FILE64_NAME=CE-SharepointOnlineEnforcer-setup64.exe

ifeq ($(BUILDTYPE),)
	BUILDTYPE=dev
endif

ifeq ($(VERSION_BUILD),)
	VERSION_BUILD=1
endif

include $(NLBUILDROOT)/install/scripts/Makefile.install
