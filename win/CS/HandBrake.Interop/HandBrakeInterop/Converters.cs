﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Converters.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Defines the Converters type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using HandBrake.Interop.Model;

namespace HandBrake.Interop
{
	using System;
	using System.Collections.Generic;

	using HandBrake.Interop.HbLib;
	using HandBrake.Interop.Model.Encoding;
	using HandBrake.Interop.SourceData;

	public static class Converters
	{
		/// <summary>
		/// Video Frame Rates
		/// </summary>
		private static Dictionary<double, int> vrates = new Dictionary<double, int>
		{
			{5, 5400000},
			{10, 2700000},
			{12, 2250000},
			{15, 1800000},
			{23.976, 1126125},
			{24, 1125000},
			{25, 1080000},
			{29.97, 900900}
		};

		/// <summary>
		/// Convert Framerate to Video Rates
		/// </summary>
		/// <param name="framerate">
		/// The framerate.
		/// </param>
		/// <returns>
		/// The vrate if a valid framerate is passed in.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when framerate is invalid.
		/// </exception>
		public static int FramerateToVrate(double framerate)
		{
			if (!vrates.ContainsKey(framerate))
			{
				throw new ArgumentException("Framerate not recognized.", "framerate");
			}

			return vrates[framerate];
		}

		/// <summary>
		/// Convert a Mixdown object to HandBrakes native mixdown constant.
		/// </summary>
		/// <param name="mixdown">
		/// The mixdown.
		/// </param>
		/// <returns>
		/// NativeContstant that represents the mixdown.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown for an invalid mixodown.
		/// </exception>
		public static int MixdownToNative(Mixdown mixdown)
		{
			if (mixdown == Mixdown.Auto)
			{
				throw new ArgumentException("Cannot convert Auto to native.");
			}

			switch (mixdown)
			{
				case Mixdown.None:
					return NativeConstants.HB_AMIXDOWN_NONE;
				case Mixdown.DolbyProLogicII:
					return NativeConstants.HB_AMIXDOWN_DOLBYPLII;
				case Mixdown.DolbySurround:
					return NativeConstants.HB_AMIXDOWN_DOLBY;
				case Mixdown.Mono:
					return NativeConstants.HB_AMIXDOWN_MONO;
				case Mixdown.SixChannelDiscrete:
					return NativeConstants.HB_AMIXDOWN_6CH;
				case Mixdown.Stereo:
					return NativeConstants.HB_AMIXDOWN_STEREO;
			}

			return 0;
		}

		/// <summary>
		/// Convert an native internal handbrake mixdown to a local mixdown enum.
		/// </summary>
		/// <param name="mixdown">
		/// The mixdown.
		/// </param>
		/// <returns>
		/// A mixdown object.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// thrown when mixdown is invalid.
		/// </exception>
		public static Mixdown NativeToMixdown(int mixdown)
		{
			switch (mixdown)
			{
				case NativeConstants.HB_AMIXDOWN_NONE:
					return Mixdown.None;
				case NativeConstants.HB_AMIXDOWN_MONO:
					return Mixdown.Mono;
				case NativeConstants.HB_AMIXDOWN_STEREO:
					return Mixdown.Stereo;
				case NativeConstants.HB_AMIXDOWN_DOLBY:
					return Mixdown.DolbySurround;
				case NativeConstants.HB_AMIXDOWN_DOLBYPLII:
					return Mixdown.DolbyProLogicII;
				case NativeConstants.HB_AMIXDOWN_6CH:
					return Mixdown.SixChannelDiscrete;
			}

			throw new ArgumentException("Unrecognized mixdown: " + mixdown, "mixdown");
		}

		/// <summary>
		/// Gets the native code for the given encoder.
		/// </summary>
		/// <param name="encoder">The audio encoder to convert.</param>
		/// <returns>The native code for the encoder.</returns>
		public static uint AudioEncoderToNative(AudioEncoder encoder)
		{
			switch (encoder)
			{
				case AudioEncoder.Passthrough:
					return NativeConstants.HB_ACODEC_AUTO_PASS;
				case AudioEncoder.Ac3Passthrough:
					return NativeConstants.HB_ACODEC_AC3_PASS;
				case AudioEncoder.Ac3:
					return NativeConstants.HB_ACODEC_AC3;
				case AudioEncoder.Faac:
					return NativeConstants.HB_ACODEC_FAAC;
				case AudioEncoder.ffaac:
					return NativeConstants.HB_ACODEC_FFAAC;
				case AudioEncoder.AacPassthru:
					return NativeConstants.HB_ACODEC_AAC_PASS;
				case AudioEncoder.Lame:
					return NativeConstants.HB_ACODEC_LAME;
				case AudioEncoder.Mp3Passthru:
					return NativeConstants.HB_ACODEC_MP3_PASS;
				case AudioEncoder.DtsPassthrough:
					return NativeConstants.HB_ACODEC_DCA_PASS;
				case AudioEncoder.DtsHDPassthrough:
					return NativeConstants.HB_ACODEC_DCA_HD_PASS;
				case AudioEncoder.Vorbis:
					return NativeConstants.HB_ACODEC_VORBIS;
			}

			return 0;
		}

		/// <summary>
		/// Convert Native HB Internal Audio int to a AudioCodec model.
		/// </summary>
		/// <param name="codec">
		/// The codec.
		/// </param>
		/// <returns>
		/// An AudioCodec object.
		/// </returns>
		public static AudioCodec NativeToAudioCodec(uint codec)
		{
			switch (codec)
			{
				case NativeConstants.HB_ACODEC_AC3:
					return AudioCodec.Ac3;
				case NativeConstants.HB_ACODEC_DCA:
					return AudioCodec.Dts;
				case NativeConstants.HB_ACODEC_DCA_HD:
					return AudioCodec.DtsHD;
				case NativeConstants.HB_ACODEC_LAME:
				case NativeConstants.HB_ACODEC_MP3:
					return AudioCodec.Mp3;
				case NativeConstants.HB_ACODEC_FAAC:
				case NativeConstants.HB_ACODEC_FFAAC:
				case NativeConstants.HB_ACODEC_CA_AAC:
				case NativeConstants.HB_ACODEC_CA_HAAC:
					return AudioCodec.Aac;
				default:
					return AudioCodec.Other;
			}
		}

		/// <summary>
		/// Converts a native HB encoder structure to an Encoder model.
		/// </summary>
		/// <param name="encoder">The structure to convert.</param>
		/// <returns>The converted model.</returns>
		public static HBVideoEncoder NativeToVideoEncoder(hb_encoder_s encoder)
		{
			var result = new HBVideoEncoder
			{
				Id = encoder.encoder,
				ShortName = encoder.short_name,
				DisplayName = encoder.human_readable_name,
				CompatibleContainers = Container.None
			};

			if ((encoder.muxers & NativeConstants.HB_MUX_MKV) > 0)
			{
				result.CompatibleContainers = result.CompatibleContainers | Container.Mkv;
			}

			if ((encoder.muxers & NativeConstants.HB_MUX_MP4) > 0)
			{
				result.CompatibleContainers = result.CompatibleContainers | Container.Mp4;
			}

			return result;
		}

		/// <summary>
		/// Converts a native HB encoder structure to an Encoder model.
		/// </summary>
		/// <param name="encoder">The structure to convert.</param>
		/// <returns>The converted model.</returns>
		public static HBAudioEncoder NativeToAudioEncoder(hb_encoder_s encoder)
		{
			var result = new HBAudioEncoder
				{
					Id = encoder.encoder,
					ShortName = encoder.short_name,
					DisplayName = encoder.human_readable_name,
					CompatibleContainers = Container.None
				};

			if ((encoder.muxers & NativeConstants.HB_MUX_MKV) > 0)
			{
				result.CompatibleContainers = result.CompatibleContainers | Container.Mkv;
			}

			if ((encoder.muxers & NativeConstants.HB_MUX_MP4) > 0)
			{
				result.CompatibleContainers = result.CompatibleContainers | Container.Mp4;
			}

			result.QualityLimits = Encoders.GetAudioQualityLimits(encoder.encoder);
			result.DefaultQuality = HBFunctions.hb_get_default_audio_quality((uint)encoder.encoder);
			result.CompressionLimits = Encoders.GetAudioCompressionLimits(encoder.encoder);
			result.DefaultCompression = HBFunctions.hb_get_default_audio_compression((uint) encoder.encoder);

			return result;
		}

		/// <summary>
		/// Converts a native HB mixdown structure to a Mixdown model.
		/// </summary>
		/// <param name="mixdown">The structure to convert.</param>
		/// <returns>The converted model.</returns>
		public static HBMixdown NativeToMixdown(hb_mixdown_s mixdown)
		{
			return new HBMixdown
			    {
					Id = mixdown.amixdown,
					ShortName = mixdown.short_name,
					DisplayName = mixdown.human_readable_name
			    };
		}
	}
}
