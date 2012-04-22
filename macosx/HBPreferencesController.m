/**
 * @file
 * Implementation of class HBPreferencesController.
 */

#import "HBPreferencesController.h"
#define TOOLBAR_GENERAL     @"TOOLBAR_GENERAL"
#define TOOLBAR_PICTURE     @"TOOLBAR_PICTURE"
#define TOOLBAR_AUDIO       @"TOOLBAR_AUDIO"
#define TOOLBAR_ADVANCED    @"TOOLBAR_ADVANCED"

/**
 * This class controls the preferences window of HandBrake. Default values for
 * all preferences and user defaults are specified in class method
 * @c registerUserDefaults. The preferences window is loaded from
 * Preferences.nib file when HBPreferencesController is initialized.
 *
 * All preferences are bound to user defaults in Interface Builder, therefore
 * no getter/setter code is needed in this file (unless more complicated
 * preference settings are added that cannot be handled with Cocoa bindings).
 */

@interface HBPreferencesController (Private)

- (void) setPrefView: (id) sender;

@end

@implementation HBPreferencesController

/**
 * Registers default values to user defaults. This is called immediately
 * when HandBrake starts, from [HBController init].
 */
+ (void)registerUserDefaults
{
    NSString *desktopDirectory =  [@"~/Desktop" stringByExpandingTildeInPath];

    [[NSUserDefaults standardUserDefaults] registerDefaults:[NSDictionary dictionaryWithObjectsAndKeys:
        @"YES",             @"CheckForUpdates",
        @"English",         @"DefaultLanguage",
        @"NO",              @"DefaultMpegName",
        @"YES",             @"DefaultCrf",
        @"",                @"DefAdvancedx264Flags",
        @"YES",             @"DefaultPresetsDrawerShow",
        desktopDirectory,   @"LastDestinationDirectory",
        desktopDirectory,   @"LastSourceDirectory",
        @"NO",              @"DefaultAutoNaming",
        @"NO",              @"DisableDvdAutoDetect",
        @"Alert Window",    @"AlertWhenDone",
        @"1",               @"LoggingLevel",
        @"4:10:15:9:10:35:9",    @"DecombCustomString",
        @"NO",    @"EncodeLogLocation",
        nil]];
}

/**
 * Initializes the preferences controller by loading Preferences.nib file.
 */
- (id)init
{
    if (self = [super initWithWindowNibName:@"Preferences"])
    {
        NSAssert([self window], @"[HBPreferencesController init] window outlet is not connected in Preferences.nib");
    }
    return self;
}

- (void) awakeFromNib
{
    NSToolbar * toolbar = [[[NSToolbar alloc] initWithIdentifier: @"Preferences Toolbar"] autorelease];
    [toolbar setDelegate: self];
    [toolbar setAllowsUserCustomization: NO];
    [toolbar setDisplayMode: NSToolbarDisplayModeIconAndLabel];
    [toolbar setSizeMode: NSToolbarSizeModeRegular];
    [[self window] setToolbar: toolbar];

    [toolbar setSelectedItemIdentifier: TOOLBAR_GENERAL];
    [self setPrefView:nil];
}

- (NSToolbarItem *) toolbar: (NSToolbar *) toolbar itemForItemIdentifier: (NSString *) ident
                    willBeInsertedIntoToolbar: (BOOL) flag
{
    NSToolbarItem * item;
    item = [[[NSToolbarItem alloc] initWithItemIdentifier: ident] autorelease];

    if ([ident isEqualToString: TOOLBAR_GENERAL])
    {
        [item setLabel: NSLocalizedString(@"General", "General")];
        [item setImage: [NSImage imageNamed: @"NSPreferencesGeneral"]];
        [item setTarget: self];
        [item setAction: @selector(setPrefView:)];
        [item setAutovalidates: NO];
    }
    else if ([ident isEqualToString: TOOLBAR_PICTURE])
    {
        [item setLabel: NSLocalizedString(@"Picture", "Picture")];
        [item setImage: [NSImage imageNamed: @"pref-picture"]];
        [item setTarget: self];
        [item setAction: @selector(setPrefView:)];
        [item setAutovalidates: NO];
    }
    else if ([ident isEqualToString: TOOLBAR_AUDIO])
    {
        [item setLabel: NSLocalizedString(@"Audio", "Audio")];
        [item setImage: [NSImage imageNamed: @"pref-audio"]];
        [item setTarget: self];
        [item setAction: @selector(setPrefView:)];
        [item setAutovalidates: NO];
    }
    else if ([ident isEqualToString: TOOLBAR_ADVANCED])
    {
        [item setLabel: NSLocalizedString(@"Advanced", "Advanced")];
        [item setImage: [NSImage imageNamed: @"NSAdvanced"]];
        [item setTarget: self];
        [item setAction: @selector(setPrefView:)];
        [item setAutovalidates: NO];
    }
    else
    {
        return nil;
    }

    return item;
}

- (NSArray *) toolbarSelectableItemIdentifiers: (NSToolbar *) toolbar
{
    return [self toolbarDefaultItemIdentifiers: toolbar];
}

- (NSArray *) toolbarDefaultItemIdentifiers: (NSToolbar *) toolbar
{
    return [self toolbarAllowedItemIdentifiers: toolbar];
}

- (NSArray *) toolbarAllowedItemIdentifiers: (NSToolbar *) toolbar
{
    return [NSArray arrayWithObjects: TOOLBAR_GENERAL, TOOLBAR_PICTURE,
                                        TOOLBAR_AUDIO, TOOLBAR_ADVANCED, nil];
}

@end

@implementation HBPreferencesController (Private)

- (void) setPrefView: (id) sender
{
    NSView * view = fGeneralView;
    if( sender )
    {
        NSString * identifier = [sender itemIdentifier];
        if( [identifier isEqualToString: TOOLBAR_PICTURE] )
            view = fPictureView;
        else if( [identifier isEqualToString: TOOLBAR_AUDIO] )
            view = fAudioView;
        else if( [identifier isEqualToString: TOOLBAR_ADVANCED] )
            view = fAdvancedView;
        else;
    }

    NSWindow * window = [self window];
    if( [window contentView] == view )
        return;

    NSRect windowRect = [window frame];
    CGFloat difference = ( [view frame].size.height - [[window contentView] frame].size.height ) * [window userSpaceScaleFactor];
    windowRect.origin.y -= difference;
    windowRect.size.height += difference;

    [view setHidden: YES];
    [window setContentView: view];
    [window setFrame: windowRect display: YES animate: YES];
    [view setHidden: NO];

    //set title label
    if( sender )
        [window setTitle: [sender label]];
    else
    {
        NSToolbar * toolbar = [window toolbar];
        NSString * itemIdentifier = [toolbar selectedItemIdentifier];
        for( NSToolbarItem * item in [toolbar items] )
            if( [[item itemIdentifier] isEqualToString: itemIdentifier] )
            {
                [window setTitle: [item label]];
                break;
            }
    }
}

@end
