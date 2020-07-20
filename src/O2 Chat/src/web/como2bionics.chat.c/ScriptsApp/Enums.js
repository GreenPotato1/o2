'use strict';

var Enums = {
    CallResultStatus: {
        Success: 0,
        AccessDenied: 1,
        ValidationFailed: 1,
        Warning: 2
      },
    ChatSessionInviteType: {
        Department: 1,
        Agent: 2
      },
    ChatSessionStatus: {
        Queued: 0,
        Active: 1,
        Completed: 2
      },
    ChatMessageSender: {
        System: 1,
        Visitor: 2,
        Agent: 3
      },
    MediaSupport: {
        NotSupported: 0,
        Audio: 1,
        Video: 2
      },
    MediaCallStatus: {
        None: 0,
        ProposedByAgent: 1,
        AcceptedByVisitor: 2,
        Established: 3
      },
    WindowMode: {
        popout: 'popout',
        iframe: 'iframe'
      },
    ConnectionState: {
        initial: 0,
        connected: 1,
        disconnected: 2,
        reconnecting: 3
      },
    ChatWidgetLocation: {
        TopLeft: 1,
        TopRight: 2,
        BottomLeft: 3,
        BottomRight: 4
      },
    ObjectStatus: {
        Active: 0,
        Disabled: 1,
        Deleted: 2,
        NotConfirmed: 3
      },
    VisitorSendTranscriptMode: {
        Ask: 0,
        Always: 1,
        Never: 2
      },
    CannedMessageType: {
        Personal: 0,
        Department: 1
      }
  };